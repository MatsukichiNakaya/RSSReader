using System;
using System.Collections.Generic;

// Must System.Data.SQLite import.
using System.Data.SQLite;

namespace Project.DataBase
{
    public class SQLite : IDisposable
    {
        /// <summary>SQLiteDBへのコネクション</summary>
        private SQLiteConnection Connection { get; set; } = null;

        /// <summary>
        /// Insert, Update時のトランザクション用コマンド
        /// </summary>
        private SQLiteCommand Command { get; set; } = null;

        /// <summary>トランザクションの状態</summary>
        public SQLiteTransactionState TransactionState { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataSource"></param>
        public SQLite(String dataSource)
        {
            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dataSource };
            this.Connection = new SQLiteConnection(sqlConnectionSb.ToString());
            this.TransactionState = SQLiteTransactionState.Stop;
        }

        /// <summary>
        /// Disopse
        /// </summary>
        public void Dispose()
        {
            if (this.Connection == null) { return; }
            if (this.Connection.State == System.Data.ConnectionState.Open)
            {
                this.Connection.Close();
            }
            this.Connection.Dispose();
        }

        /// <summary>DB接続開く</summary>
        public void Open() => this.Connection.Open();

        /// <summary>DB接続閉じる</summary>
        public void Close() => this.Connection.Close();

        /// <summary>
        /// 射影コマンド
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Dictionary<String, List<String>> Select(String command)
        {
            var result = new Dictionary<String, List<String>>();
            using (SQLiteCommand cmd = this.Connection.CreateCommand())
            {
                cmd.CommandText = command;
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    // データがないので初期化のまま返す。
                    if (!reader.HasRows) { return result; }
                    // 最初の行を作成する
                    reader.Read();
                    var pairs = GetColumns(reader);
                    foreach (var pair in pairs)
                    {
                        result.Add(pair.Key, new List<String>());
                        result[pair.Key].Add(pair.Value);
                    }
                    // 次の行からは値の設定のみ
                    while (reader.Read())
                    {
                        pairs = GetColumns(reader);
                        foreach (var pair in pairs)
                        {
                            result[pair.Key].Add(pair.Value);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 行の値を列名と値のペアにして返す
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<String, String>> GetColumns(SQLiteDataReader reader)
        {
            for (Int32 i = 0; i < reader.FieldCount; i++)
            {
                yield return new KeyValuePair<String, String>(reader.GetName(i) ,reader[i].ToString());
            }
        }

        /// <summary>
        /// トランザクション開始
        /// </summary>
        public void BeginTransaction()
        {
            this.Command = this.Connection.CreateCommand();
            this.Command.Transaction = this.Connection.BeginTransaction();
            this.TransactionState = SQLiteTransactionState.Running;
        }

        /// <summary>
        /// トランザクション終了
        /// </summary>
        public void EndTransaction(Boolean isCommit)
        {
            if (isCommit)
            {
                this.Command.Transaction.Commit();
            }
            else
            {
                this.Command.Transaction.Rollback();
            }
            this.Command.Dispose();
            this.TransactionState = SQLiteTransactionState.Stop;
        }

        /// <summary>
        /// 更新コマンド
        /// </summary>
        /// <param name="command"></param>
        public Int32 Update(String command)
        {
            if(this.Connection.State == System.Data.ConnectionState.Closed) { return 0; }

            // トランザクションの開始と終了に関してのコマンド呼び出し有無で分岐
            if (this.TransactionState == SQLiteTransactionState.Stop)
            {   // トランザクション開始していない
                using (SQLiteCommand cmd = this.Connection.CreateCommand())
                {
                    cmd.CommandText = command;
                    // Implicit begin transaction
                    return cmd.ExecuteNonQuery();
                    // Implicit commit
                }
            }
            else
            {   // トランザクション開始している
                this.Command.CommandText = command;
                return this.Command.ExecuteNonQuery();
            }
        }
    }
}
