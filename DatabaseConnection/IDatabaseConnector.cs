using System;
using System.Collections.Generic;

namespace DatabaseConnection
{
    public interface IDatabaseConnector
    {
        /// <summary>
        /// ステートメントを実行し影響を受けた行数を返します。
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>影響を受けた行数</returns>
        int ExecuteNonQuery(string sql);

        /// <summary>
        /// ステートメントを実行し影響を受けた行数を返します。
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">パラメータ</param>
        /// <returns>影響を受けた行数</returns>
        int ExecuteNonQuery(string sql, IDictionary<string, object> parameters);

        /// <summary>
        /// ステートメントを実行し取得された行を指定の処理で変換して返します。
        /// </summary>
        /// <typeparam name="T">返却する型</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="convert">変換処理</param>
        /// <returns>変換された型のコレクション</returns>
        IEnumerable<T> ExecuteQuery<T>(string sql, Func<ReadRow, T> convert);

        /// <summary>
        /// ステートメントを実行し取得された行を指定の処理で変換して返します。
        /// </summary>
        /// <typeparam name="T">返却する型</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">パラメータ</param>
        /// <param name="convert">変換処理</param>
        /// <returns>変換された型のコレクション</returns>
        IEnumerable<T> ExecuteQuery<T>(
            string sql, IDictionary<string, object> parameters, Func<ReadRow, T> convert);

        /// <summary>
        /// コードブロックをトランザクションにします。
        /// </summary>
        /// <returns>トランザクション</returns>
        IDisposable SetTransaction();

        /// <summary>
        /// コードブロック内の操作が正常に完了したことを示します。
        /// </summary>
        void CompleteTransaction();
    }
}
