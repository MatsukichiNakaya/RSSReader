using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Extention
{
    /// <summary>配列に対する拡張関数クラス</summary>
    /// <remarks>
    /// Linqのメソッドたちをわざわざラップして使う。
    /// ほぼそのままのメソッドもある。
    /// </remarks>
    public static class ArrayExtention
    {
        #region 配列の形状変換
        /// <summary>
        /// 列挙子 <typeparamref name="T"/> を配列 <typeparamref name="T"/>[] に変換する
        /// </summary>
        /// <typeparam name="T">列挙子を構成する型</typeparam>
        /// <param name="src">元になる配列</param>
        /// <returns>配列 <typeparamref name="T"/>[]</returns>
        public static T[] ToIndexed<T>(this IEnumerable<T> src) => Enumerable.ToArray(src);

        /// <summary>
        /// コレクションを実装する配列 <typeparamref name="T"/> を 
        /// 列挙子 <typeparamref name="T"/> に変換する
        /// </summary>
        /// <typeparam name="T">列挙子を構成する型</typeparam>
        /// <param name="src">元になる配列</param>
        /// <returns>列挙子 <typeparamref name="T"/></returns>
        public static IEnumerable<T> ToEnumerate<T>(this IEnumerable<T> src) => src;

        /// <summary>
        /// 配列を指定した長さにする
        /// </summary>
        /// <typeparam name="T">列挙子を構成する型</typeparam>
        /// <param name="src">元になる配列</param>
        /// <param name="length">配列の長さ</param>
        /// <returns>リサイズ後の長さの配列</returns>
        /// <remarks>
        /// 元の長さの配列の部分は値が保持され
        /// でない部分はdefault(<typeparamref name="T"/>)が適用される
        /// </remarks>
        public static T[] Resize<T>(this IEnumerable<T> src, Int32 length)
        {   // デフォルト値で初期化
            var result = Enumerable.Repeat(default(T), length).ToArray();
            // サイズを指定して内容をコピー
            Int32 typeSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            Buffer.BlockCopy(src as Array ?? src.ToArray(), 0, result, 0, 
                                Math.Min(src.Count(), length) * typeSize);
            return result;
        }

        /// <summary>
        /// すべての値が 型 <typeparamref name="T"/> のデフォルト値を持つ配列を作成する
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="length">配列の長さ</param>
        /// <returns>>指定した長さですべて初期値の配列 <typeparamref name="T"/>[]</returns>
        public static T[] NewArray<T>(Int32 length)
            => Enumerable.Repeat(default(T), length).ToArray();

        /// <summary>
        /// すべて同じ値を持つ配列 <typeparamref name="T"/>[] を作成する
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="element">配列の値</param>
        /// <param name="length">配列の長さ</param>
        /// <returns>指定した長さですべて同じ値の配列 <typeparamref name="T"/>[]</returns>
        public static T[] NewArray<T>(T element, Int32 length)
            => Enumerable.Repeat(element, length).ToArray();

        /// <summary>
        /// 二次元配列の初期化
        /// </summary>
        /// <param name="row">行数</param>
        /// <param name="col">列数</param>
        public static T[][] NewJagArray<T>(Int32 row, Int32 col)
            // 0 ～ rowの配列作成　 作成された配列と同じ数の配列を作成する
            => Enumerable.Range(0, row).Select(_ => new T[col]).ToIndexed();

        /// <summary>
        /// 配列に対して関数を割当てて計算。それぞれの返り値を返す
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <typeparam name="TResult">返り値の型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="func">関数</param>
        public static IEnumerable<TResult> GroupBy<T, TResult>(this IEnumerable<T> src,
                                                                Func<T, TResult> func)
            => src.Select(arg => func(arg));

        /// <summary>
        /// 配列に対して関数を割当てて計算。それぞれの返り値を返す
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <typeparam name="TResult">返り値の型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="func">関数</param>
        /// <remarks>インデックスを使用するオーバーロード</remarks>
        public static IEnumerable<TResult> GroupBy<T, TResult>(this IEnumerable<T> src,
                                                                Func<T, Int32, TResult> func)
        {
            Int32 i = -1;
            foreach (T item in src)
            {   // 0オリジンでループ開始
                checked { i++; }
                yield return func(item, i);
            }
        }
        #endregion

        #region データの取得
        /// <summary>
        /// 配列の長さを取得する
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">参照する配列</param>
        /// <returns>配列の長さ</returns>
        public static Int32 Count<T>(this IEnumerable<T> src)
        {
            if (src == null) { throw new NullReferenceException(); }
            return (src as ICollection<T>)?.Count ?? Enumerable.Count(src);
        }

        /// <summary>
        /// 条件式にに対応する要素数を返す
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">参照する配列</param>
        /// <param name="predicate">条件式</param>
        /// <returns>条件に一致した要素の個数</returns>
        public static Int32 CountIf<T>(this IEnumerable<T> src,
                                        Func<T, Boolean> predicate)
            => Enumerable.Count(src, predicate);

        /// <summary>
        /// 配列の最初の要素を取得します
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">参照する配列</param>
        public static T First<T>(this IEnumerable<T> src) => Enumerable.First(src);

        /// <summary>条件式に一致した最初の値を取得</summary>
        /// <param name="src">参照する配列</param>
        /// <param name="predicate">条件式</param>
        /// <remarks>一致しない場合はデフォルト値返却</remarks>
        public static T FirstIf<T>(this IEnumerable<T> src,
                                    Func<T, Boolean> predicate)
            => Enumerable.FirstOrDefault(src, predicate);

        /// <summary>
        /// 条件式に一致した要素の中から最初の要素のインデックスを取得
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">参照する配列</param>
        /// <param name="predicate">条件式</param>
        /// <remarks>一致しない場合は[-1]を返却</remarks>
        public static Int32 FirstIfOfIndex<T>(this IEnumerable<T> src,
                                                Func<T, Boolean> predicate)
        {
            // 条件に一致する項目を取得
            var result = from item in src.Select((val, idx) => new { val, idx })
                         where predicate(item.val)
                         select item.idx;
            // 一致した最初の項目を返す。　一致無は-1を返す
            return result.Count() > 0 ? result.First() : -1;
        }

        /// <summary>
        /// 配列末尾の要素を取得します
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">参照する配列</param>
        /// <remarks>配列末尾の要素</remarks>
        public static T Last<T>(this IEnumerable<T> src)
        {
            if (src == null) { throw new NullReferenceException(); }
            // 配列がきた場合は最後の要素を直接指定
            if (src is IList<T> array) { return array[array.Count - 1]; }

            // 最後の要素を取得
            return Enumerable.Last(src);
        }

        /// <summary>
        /// 配列内の指定位置から指定範囲の要素を取り出す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">参照する配列</param>
        /// <param name="skip">開始位置</param>
        /// <param name="length">取得長さ</param>
        public static T[] SkipTake<T>(this IEnumerable<T> src, Int32 skip, Int32 length)
        {
            if (skip < 0) { return new T[0]; }
            if (length < 0) { return new T[0]; }
            Int32 min = Math.Min(src.Count() - skip, length);
            var result = new T[min];
            Int32 typeSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            Buffer.BlockCopy(src as Array ?? src.ToArray(), skip * typeSize, result, 0, min * typeSize);
            return result;
        }

        /// <summary>
        /// ジャグ配列中の列最大長を取得
        /// </summary>
        /// <param name="src">ジャグ配列</param>
        public static Int32 ColumnsMax<T>(this T[][] src)
            // 配列の長さを取得して最大値を返す
            => Enumerable.Max(src, row => row.Length);

        /// <summary>
        /// 要素を指定して、要素の最小値を返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TResult Minimum<T, TResult>(this IEnumerable<T> src,
                                                  Func<T, TResult> selector)
            => Enumerable.Min(src, selector);

        /// <summary>
        /// 配列内から条件に一致するもの(true)を抽出する
        /// </summary>
        /// <param name="src">参照する配列</param>
        /// <param name="predicate">条件式</param>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> src,
                                               Func<T, Boolean> predicate)
            => Enumerable.Where(src, predicate);

        /// <summary>
        /// 配列内から条件に一致するもの(true)を抽出する
        /// </summary>
        /// <param name="src">参照する配列</param>
        /// <param name="predicate">条件式</param>
        /// <remarks>インデックスを付ける条件式のオーバーロード</remarks>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> src,
                                                Func<T, Int32, Boolean> predicate)
            => Enumerable.Where(src, (val, idx) => predicate(val, idx));

        /// <summary>
        /// 配列内から同じものを取り除く
        /// </summary>
        /// <param name="src">参照する配列</param>
        public static IEnumerable<T> SameFilter<T>(this IEnumerable<T> src)
            => Enumerable.Distinct(src);

        /// <summary>配列の中がすべてTrueか？</summary>
        /// <param name="src">参照する配列</param>
        /// <returns>すべてtrue: true / 一つでもfalseがある: false</returns>
        public static Boolean IsTrue(this IEnumerable<Boolean> src)
            // falseの項目を取得する。　そしてカウントが０ならtrue
            => src.Filter(b => !b).Count() == 0;

        /// <summary>
        /// 連続した値をステップして作成
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static Int32[] SequenceValue(Int32 start, Int32 end, Int32 step)
        {
            Int32[] result = new Int32[end - start];
            Int32 startValue = start;
            for (Int32 i = 0; i < result.Length; i++)
            {
                result[i] = startValue;
                startValue += step;
            }
            return result;
        }
        #endregion

        #region ソート
        /// <summary>
        /// 昇順の並べ替え
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        public static IEnumerable<T> SortOfAsc<T>(this IEnumerable<T> src)
            => from item in src orderby item ascending select item;

        /// <summary>
        /// 昇順の並べ替えを行う
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <typeparam name="TKey">並べ替え要素の型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="selector">条件式</param>
        /// <remarks>並べ替え条件式のオーバーロード</remarks>
        public static IEnumerable<T> SortOfAsc<T, TKey>(this IEnumerable<T> src,
                                                        Func<T, TKey> selector)
            => src.OrderBy(selector);

        /// <summary>
        /// 降順の並べ替え
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        public static IEnumerable<T> SortOfDesc<T>(this IEnumerable<T> src)
            => from item in src orderby item descending select item;

        /// <summary>
        /// 降順の並べ替えを行う
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <typeparam name="TKey">並べ替え要素の型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="selector">条件式</param>
        /// <remarks>並べ替え条件式のオーバーロード</remarks>
        public static IEnumerable<T> SortOfDesc<T, TKey>(this IEnumerable<T> src,
                                                         Func<T, TKey> selector)
            => src.OrderByDescending(selector);

        /// <summary>
        /// 要素にインデックスを付加して昇順ソートを行い、インデックスを取得する
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        public static IEnumerable<Int32> IndexOfAscSort<T>(this IEnumerable<T> src)
            => from item in src.Select((value, index) => new { value, index })
               orderby item.value ascending
               select item.index;

        /// <summary>
        /// 要素にインデックスを付加して降順ソートを行い、インデックスを取得する
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        public static IEnumerable<Int32> IndexOfDescSort<T>(this IEnumerable<T> src)
            => from item in src.Select((value, index) => new { value, index })
               orderby item.value descending
               select item.index;

        /// <summary>
        /// 要素にインデックスを付加して昇順ソートを行い、指定数のインデックスを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <param name="count">取得数</param>
        public static Int32[] IndexRankingOfAscSort<T>(this IEnumerable<T> src, Int32 count)
            => GetRankIndex(src, count, true);

        /// <summary>
        /// 要素にインデックスを付加して降順ソートを行い、指定数のインデックスを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <param name="count">取得数</param>
        public static Int32[] IndexRankingOfDescSort<T>(this IEnumerable<T> src, Int32 count)
            => GetRankIndex(src, count, false);

        /// <summary>
        /// 配列内のアイテムを昇順、または降順で並べ替えてトップから必要数のインデックスを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <param name="count">取得数</param>
        /// <param name="sortType">昇順(true), 降順(false)</param>
        private static Int32[] GetRankIndex<T>(IEnumerable<T> src, Int32 count, Boolean isAsc)
        {
            // 0 の場合は引き出す必要がないためそのまま返す 
            if (count == 0) { return new Int32[count]; }
            // 昇順(true)または降順(false)でデータの並べ替え
            return (isAsc ?  src.IndexOfAscSort() : src.IndexOfDescSort()
                    // 必要な数だけ取り出す
                    ).Take(count).ToArray();
        }
        #endregion
    }
}
