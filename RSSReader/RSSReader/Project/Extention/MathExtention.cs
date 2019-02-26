using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Extention
{
    /// <summary>計算を行う拡張関数クラス</summary>
    public static class MathExtention
    {
        #region 集計
        /// <summary>
        /// 数値の合計(実数)
        /// </summary>
        /// <param name="src">配列</param>
        public static Double Total(this IEnumerable<Double> src) => src.Sum();

        /// <summary>
        /// 数値の合計(整数)
        /// </summary>
        /// <param name="src">配列</param>
        public static Int32 Total(this IEnumerable<Int32> src) => src.Sum();

        /// <summary>
        /// 関数を用いた数値の合計(実数) Σ
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="func">関数</param>
        public static Double Sigma<T>(this IEnumerable<T> src, Func<T, Double> func)
            => src.Select(arg => func(arg)).Sum();

        /// <summary>
        /// 関数を用いた数値の合計(整数) Σ
        /// </summary>
        /// <typeparam name="T">配列を構成する型</typeparam>
        /// <param name="src">配列</param>
        /// <param name="func">関数</param>
        public static Int32 Sigma<T>(this IEnumerable<T> src, Func<T, Int32> func)
            => src.Select(arg => func(arg)).Sum();
        #endregion

        #region 平均
        /// <summary>
        /// 配列内の要素の平均値を計算
        /// </summary>
        /// <param name="src">配列</param>
        /// <returns>平均値</returns>
        public static Double Average(this IEnumerable<Double> src)
        {
            return src.Sum() / src.Count();
        }

        /// <summary>
        /// 配列内の要素の平均値を計算
        /// </summary>
        /// <param name="src">配列</param>
        /// <returns>平均値</returns>
        public static Double AverageIf(this IEnumerable<Double> src,
                                            Func<Double, Boolean> selector)
        {
            var selectArray = src.Where(val => selector(val));  // 条件式による抽出
            return selectArray.Sum() / selectArray.Count();     // 平均値計算
        }
        #endregion

        #region ビット計算
        /// <summary>
        /// ビットフラグ判定
        /// </summary>
        /// <param name="value">数値</param>
        /// <param name="bit">ビット(1～)</param>
        public static Boolean BitFlag(this UInt32 value, Int32 bit)
            // 受け取った数値の指定されたビットが立っているか確認
            => (value & (1 << bit - 1)) != 0;
        
        /// <summary>
        /// ビットフラグ判定
        /// </summary>
        /// <param name="value">数値</param>
        /// <param name="bit">ビット(1～)</param>
        /// <remarks>符号付への対応オーバーロード</remarks>
        public static Boolean BitFlag(this Int32 value, Int32 bit)
        {   // 受け取った数値の指定されたビットが立っているか確認
            return (value & (1 << bit - 1)) != 0;
        }

        /// <summary>
        /// ビット数カウント
        /// </summary>
        /// <param name="value">カウントする数値</param>
        /// <remarks>
        /// マイナス値が入らないように
        /// 符号無での値を設定
        /// </remarks>
        public static Int32 BitCount(this UInt32 value)
        {
            var bits = (Int32)value;
            bits = (bits & 0x55555555) + (bits >> 1 & 0x55555555);
            bits = (bits & 0x33333333) + (bits >> 2 & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + (bits >> 8 & 0x00ff00ff);
            return (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
        }

        /// <summary>数値をBit(0,1)の配列に変換</summary>
        public static UInt32[] ToBitArray(this UInt32 value)
        {
            return Array.ConvertAll(Convert.ToString(value, 2).ToCharArray(), c => ((UInt32)c) - 48);
        }
        #endregion

        #region 実数桁あわせ関数 floorとceilingが感覚的に馴染まない為作成
        /// <summary>
        /// 指定した精度の数値に四捨五入する
        /// </summary>
        /// <param name="value">元の値</param>
        /// <param name="digits">有効桁数</param>
        /// <remarks>
        /// MathクラスのFloorとCeillngの挙動が
        /// 慣れ親しんだ四捨五入ではないので作成。
        /// (以下Down, Upも同様)
        /// </remarks>
        public static Double Round(this Double value, Int32 digits)
        {
            Double coef = Math.Pow(10, digits); //変換用係数

            return value > 0 ? Math.Floor((value * coef) + 0.5) / coef 
                             : Math.Ceiling((value * coef) - 0.5) / coef;
        }

        /// <summary>
        /// 指定した精度の数値に切捨てる
        /// </summary>
        /// <param name="value">元の値</param>
        /// <param name="digits">有効桁数</param>
        public static Double RoundDown(this Double value, Int32 digits)
        {
            Double coef = Math.Pow(10, digits);

            return value > 0 ? Math.Floor(value * coef) / coef 
                             : Math.Ceiling(value * coef) / coef;
        }

        /// <summary>
        /// 指定した精度の数値に切上げる
        /// </summary>
        /// <param name="value">元の値</param>
        /// <param name="digits">有効桁数</param>
        public static Double RoundUp(this Double value, Int32 digits)
        {
            Double coef = Math.Pow(10, digits);

            return value > 0 ? Math.Ceiling(value * coef) / coef
                             : Math.Floor(value * coef) / coef;
        }

        /// <summary>
        /// 整数にする為の四捨五入
        /// </summary>
        /// <param name="value">元の値</param>
        /// <returns>整数値</returns>
        public static Int32 Round(this Double value)
        {
            Double coef = Math.Pow(10, 0); //変換用係数

            return value > 0 ? (Int32)(Math.Floor((value * coef) + 0.5) / coef)
                             : (Int32)(Math.Ceiling((value * coef) - 0.5) / coef);
        }

        /// <summary>
        /// 整数にする為の端数切捨て
        /// </summary>
        /// <param name="value">元の値</param>
        /// <returns>整数値</returns>
        public static Int32 RoundDown(this Double value)
        {
            Double coef = Math.Pow(10, 0);

            return value > 0 ? (Int32)(Math.Floor(value * coef) / coef)
                             : (Int32)(Math.Ceiling(value * coef) / coef);
        }

        /// <summary>
        /// 整数にする為の端数切上げ
        /// </summary>
        /// <param name="value">元の値</param>
        /// <returns>整数値</returns>
        public static Double RoundUp(this Double value)
        {
            Double coef = Math.Pow(10, 0);

            return value > 0 ? (Int32)(Math.Ceiling(value * coef) / coef)
                             : (Int32)(Math.Floor(value * coef) / coef);
        }
        #endregion

        #region Mathクラスを拡張関数にラップ
        /// <summary>
        /// 冪乗の計算
        /// </summary>
        /// <param name="value">底</param>
        /// <param name="exponent">指数</param>
        public static Double Pow(this Double value, UInt32 exponent)
            => Math.Pow(value, exponent);

        /// <summary>
        /// 冪乗の計算
        /// </summary>
        /// <param name="value">底</param>
        /// <param name="exponent">指数</param>
        public static Int32 Pow(this Int32 value, UInt32 exponent)
            => (Int32)Math.Pow(value, exponent);

        /// <summary>絶対値の取得</summary>
        /// <param name="value">元の値</param>
        public static Double Abs(this Double value) => Math.Abs(value);

        /// <summary>絶対値の取得</summary>
        /// <param name="value">元の値</param>
        public static Int32 Abs(this Int32 value) => Math.Abs(value);
        #endregion

        #region 小数点以下を取得
        /// <summary>
        /// 小数点以下の値を取得する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Double GetDecimal(this Double value)
        {
            return value % 1;
        }
        #endregion


        /// <summary>数値に変換できる？</summary>
        public static Boolean IsNumeric(this ValueType src)
        {
            // 数値型のいずれかにヒットする？
            return (src is Byte || src is Int16 || src is Int32 || src is Int64 ||
                    src is SByte || src is UInt16 || src is UInt32 || src is UInt64 ||
                    src is Decimal || src is Double || src is Single);
        }
    }
}