using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Extention
{
    /// <summary>�z��ɑ΂���g���֐��N���X</summary>
    /// <remarks>
    /// Linq�̃��\�b�h�������킴�킴���b�v���Ďg���B
    /// �قڂ��̂܂܂̃��\�b�h������B
    /// </remarks>
    public static class ArrayExtention
    {
        #region �z��̌`��ϊ�
        /// <summary>
        /// �񋓎q <typeparamref name="T"/> ��z�� <typeparamref name="T"/>[] �ɕϊ�����
        /// </summary>
        /// <typeparam name="T">�񋓎q���\������^</typeparam>
        /// <param name="src">���ɂȂ�z��</param>
        /// <returns>�z�� <typeparamref name="T"/>[]</returns>
        public static T[] ToIndexed<T>(this IEnumerable<T> src) => Enumerable.ToArray(src);

        /// <summary>
        /// �R���N�V��������������z�� <typeparamref name="T"/> �� 
        /// �񋓎q <typeparamref name="T"/> �ɕϊ�����
        /// </summary>
        /// <typeparam name="T">�񋓎q���\������^</typeparam>
        /// <param name="src">���ɂȂ�z��</param>
        /// <returns>�񋓎q <typeparamref name="T"/></returns>
        public static IEnumerable<T> ToEnumerate<T>(this IEnumerable<T> src) => src;

        /// <summary>
        /// �z����w�肵�������ɂ���
        /// </summary>
        /// <typeparam name="T">�񋓎q���\������^</typeparam>
        /// <param name="src">���ɂȂ�z��</param>
        /// <param name="length">�z��̒���</param>
        /// <returns>���T�C�Y��̒����̔z��</returns>
        /// <remarks>
        /// ���̒����̔z��̕����͒l���ێ�����
        /// �łȂ�������default(<typeparamref name="T"/>)���K�p�����
        /// </remarks>
        public static T[] Resize<T>(this IEnumerable<T> src, Int32 length)
        {   // �f�t�H���g�l�ŏ�����
            var result = Enumerable.Repeat(default(T), length).ToArray();
            // �T�C�Y���w�肵�ē��e���R�s�[
            Int32 typeSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            Buffer.BlockCopy(src as Array ?? src.ToArray(), 0, result, 0, 
                                Math.Min(src.Count(), length) * typeSize);
            return result;
        }

        /// <summary>
        /// ���ׂĂ̒l�� �^ <typeparamref name="T"/> �̃f�t�H���g�l�����z����쐬����
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="length">�z��̒���</param>
        /// <returns>>�w�肵�������ł��ׂď����l�̔z�� <typeparamref name="T"/>[]</returns>
        public static T[] NewArray<T>(Int32 length)
            => Enumerable.Repeat(default(T), length).ToArray();

        /// <summary>
        /// ���ׂē����l�����z�� <typeparamref name="T"/>[] ���쐬����
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="element">�z��̒l</param>
        /// <param name="length">�z��̒���</param>
        /// <returns>�w�肵�������ł��ׂē����l�̔z�� <typeparamref name="T"/>[]</returns>
        public static T[] NewArray<T>(T element, Int32 length)
            => Enumerable.Repeat(element, length).ToArray();

        /// <summary>
        /// �񎟌��z��̏�����
        /// </summary>
        /// <param name="row">�s��</param>
        /// <param name="col">��</param>
        public static T[][] NewJagArray<T>(Int32 row, Int32 col)
            // 0 �` row�̔z��쐬�@ �쐬���ꂽ�z��Ɠ������̔z����쐬����
            => Enumerable.Range(0, row).Select(_ => new T[col]).ToIndexed();

        /// <summary>
        /// �z��ɑ΂��Ċ֐��������ĂČv�Z�B���ꂼ��̕Ԃ�l��Ԃ�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <typeparam name="TResult">�Ԃ�l�̌^</typeparam>
        /// <param name="src">�z��</param>
        /// <param name="func">�֐�</param>
        public static IEnumerable<TResult> GroupBy<T, TResult>(this IEnumerable<T> src,
                                                                Func<T, TResult> func)
            => src.Select(arg => func(arg));

        /// <summary>
        /// �z��ɑ΂��Ċ֐��������ĂČv�Z�B���ꂼ��̕Ԃ�l��Ԃ�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <typeparam name="TResult">�Ԃ�l�̌^</typeparam>
        /// <param name="src">�z��</param>
        /// <param name="func">�֐�</param>
        /// <remarks>�C���f�b�N�X���g�p����I�[�o�[���[�h</remarks>
        public static IEnumerable<TResult> GroupBy<T, TResult>(this IEnumerable<T> src,
                                                                Func<T, Int32, TResult> func)
        {
            Int32 i = -1;
            foreach (T item in src)
            {   // 0�I���W���Ń��[�v�J�n
                checked { i++; }
                yield return func(item, i);
            }
        }
        #endregion

        #region �f�[�^�̎擾
        /// <summary>
        /// �z��̒������擾����
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <returns>�z��̒���</returns>
        public static Int32 Count<T>(this IEnumerable<T> src)
        {
            if (src == null) { throw new NullReferenceException(); }
            return (src as ICollection<T>)?.Count ?? Enumerable.Count(src);
        }

        /// <summary>
        /// �������ɂɑΉ�����v�f����Ԃ�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="predicate">������</param>
        /// <returns>�����Ɉ�v�����v�f�̌�</returns>
        public static Int32 CountIf<T>(this IEnumerable<T> src,
                                        Func<T, Boolean> predicate)
            => Enumerable.Count(src, predicate);

        /// <summary>
        /// �z��̍ŏ��̗v�f���擾���܂�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        public static T First<T>(this IEnumerable<T> src) => Enumerable.First(src);

        /// <summary>�������Ɉ�v�����ŏ��̒l���擾</summary>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="predicate">������</param>
        /// <remarks>��v���Ȃ��ꍇ�̓f�t�H���g�l�ԋp</remarks>
        public static T FirstIf<T>(this IEnumerable<T> src,
                                    Func<T, Boolean> predicate)
            => Enumerable.FirstOrDefault(src, predicate);

        /// <summary>
        /// �������Ɉ�v�����v�f�̒�����ŏ��̗v�f�̃C���f�b�N�X���擾
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="predicate">������</param>
        /// <remarks>��v���Ȃ��ꍇ��[-1]��ԋp</remarks>
        public static Int32 FirstIfOfIndex<T>(this IEnumerable<T> src,
                                                Func<T, Boolean> predicate)
        {
            // �����Ɉ�v���鍀�ڂ��擾
            var result = from item in src.Select((val, idx) => new { val, idx })
                         where predicate(item.val)
                         select item.idx;
            // ��v�����ŏ��̍��ڂ�Ԃ��B�@��v����-1��Ԃ�
            return result.Count() > 0 ? result.First() : -1;
        }

        /// <summary>
        /// �z�񖖔��̗v�f���擾���܂�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <remarks>�z�񖖔��̗v�f</remarks>
        public static T Last<T>(this IEnumerable<T> src)
        {
            if (src == null) { throw new NullReferenceException(); }
            // �z�񂪂����ꍇ�͍Ō�̗v�f�𒼐ڎw��
            if (src is IList<T> array) { return array[array.Count - 1]; }

            // �Ō�̗v�f���擾
            return Enumerable.Last(src);
        }

        /// <summary>
        /// �z����̎w��ʒu����w��͈̗͂v�f�����o��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="skip">�J�n�ʒu</param>
        /// <param name="length">�擾����</param>
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
        /// �W���O�z�񒆂̗�ő咷���擾
        /// </summary>
        /// <param name="src">�W���O�z��</param>
        public static Int32 ColumnsMax<T>(this T[][] src)
            // �z��̒������擾���čő�l��Ԃ�
            => Enumerable.Max(src, row => row.Length);

        /// <summary>
        /// �v�f���w�肵�āA�v�f�̍ŏ��l��Ԃ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TResult Minimum<T, TResult>(this IEnumerable<T> src,
                                                  Func<T, TResult> selector)
            => Enumerable.Min(src, selector);

        /// <summary>
        /// �z�����������Ɉ�v�������(true)�𒊏o����
        /// </summary>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="predicate">������</param>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> src,
                                               Func<T, Boolean> predicate)
            => Enumerable.Where(src, predicate);

        /// <summary>
        /// �z�����������Ɉ�v�������(true)�𒊏o����
        /// </summary>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <param name="predicate">������</param>
        /// <remarks>�C���f�b�N�X��t����������̃I�[�o�[���[�h</remarks>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> src,
                                                Func<T, Int32, Boolean> predicate)
            => Enumerable.Where(src, (val, idx) => predicate(val, idx));

        /// <summary>
        /// �z������瓯�����̂���菜��
        /// </summary>
        /// <param name="src">�Q�Ƃ���z��</param>
        public static IEnumerable<T> SameFilter<T>(this IEnumerable<T> src)
            => Enumerable.Distinct(src);

        /// <summary>�z��̒������ׂ�True���H</summary>
        /// <param name="src">�Q�Ƃ���z��</param>
        /// <returns>���ׂ�true: true / ��ł�false������: false</returns>
        public static Boolean IsTrue(this IEnumerable<Boolean> src)
            // false�̍��ڂ��擾����B�@�����ăJ�E���g���O�Ȃ�true
            => src.Filter(b => !b).Count() == 0;

        /// <summary>
        /// �A�������l���X�e�b�v���č쐬
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

        #region �\�[�g
        /// <summary>
        /// �����̕��בւ�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�z��</param>
        public static IEnumerable<T> SortOfAsc<T>(this IEnumerable<T> src)
            => from item in src orderby item ascending select item;

        /// <summary>
        /// �����̕��בւ����s��
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <typeparam name="TKey">���בւ��v�f�̌^</typeparam>
        /// <param name="src">�z��</param>
        /// <param name="selector">������</param>
        /// <remarks>���בւ��������̃I�[�o�[���[�h</remarks>
        public static IEnumerable<T> SortOfAsc<T, TKey>(this IEnumerable<T> src,
                                                        Func<T, TKey> selector)
            => src.OrderBy(selector);

        /// <summary>
        /// �~���̕��בւ�
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�z��</param>
        public static IEnumerable<T> SortOfDesc<T>(this IEnumerable<T> src)
            => from item in src orderby item descending select item;

        /// <summary>
        /// �~���̕��בւ����s��
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <typeparam name="TKey">���בւ��v�f�̌^</typeparam>
        /// <param name="src">�z��</param>
        /// <param name="selector">������</param>
        /// <remarks>���בւ��������̃I�[�o�[���[�h</remarks>
        public static IEnumerable<T> SortOfDesc<T, TKey>(this IEnumerable<T> src,
                                                         Func<T, TKey> selector)
            => src.OrderByDescending(selector);

        /// <summary>
        /// �v�f�ɃC���f�b�N�X��t�����ď����\�[�g���s���A�C���f�b�N�X���擾����
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�z��</param>
        public static IEnumerable<Int32> IndexOfAscSort<T>(this IEnumerable<T> src)
            => from item in src.Select((value, index) => new { value, index })
               orderby item.value ascending
               select item.index;

        /// <summary>
        /// �v�f�ɃC���f�b�N�X��t�����č~���\�[�g���s���A�C���f�b�N�X���擾����
        /// </summary>
        /// <typeparam name="T">�z����\������^</typeparam>
        /// <param name="src">�z��</param>
        public static IEnumerable<Int32> IndexOfDescSort<T>(this IEnumerable<T> src)
            => from item in src.Select((value, index) => new { value, index })
               orderby item.value descending
               select item.index;

        /// <summary>
        /// �v�f�ɃC���f�b�N�X��t�����ď����\�[�g���s���A�w�萔�̃C���f�b�N�X���擾����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">�z��</param>
        /// <param name="count">�擾��</param>
        public static Int32[] IndexRankingOfAscSort<T>(this IEnumerable<T> src, Int32 count)
            => GetRankIndex(src, count, true);

        /// <summary>
        /// �v�f�ɃC���f�b�N�X��t�����č~���\�[�g���s���A�w�萔�̃C���f�b�N�X���擾����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">�z��</param>
        /// <param name="count">�擾��</param>
        public static Int32[] IndexRankingOfDescSort<T>(this IEnumerable<T> src, Int32 count)
            => GetRankIndex(src, count, false);

        /// <summary>
        /// �z����̃A�C�e���������A�܂��͍~���ŕ��בւ��ăg�b�v����K�v���̃C���f�b�N�X���擾
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">�z��</param>
        /// <param name="count">�擾��</param>
        /// <param name="sortType">����(true), �~��(false)</param>
        private static Int32[] GetRankIndex<T>(IEnumerable<T> src, Int32 count, Boolean isAsc)
        {
            // 0 �̏ꍇ�͈����o���K�v���Ȃ����߂��̂܂ܕԂ� 
            if (count == 0) { return new Int32[count]; }
            // ����(true)�܂��͍~��(false)�Ńf�[�^�̕��בւ�
            return (isAsc ?  src.IndexOfAscSort() : src.IndexOfDescSort()
                    // �K�v�Ȑ��������o��
                    ).Take(count).ToArray();
        }
        #endregion
    }
}
