using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Learning.Common.MQ
{
    /// <summary>
    ///  MQ Result
    /// </summary>
    public class PubResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public long ResultCode { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// init <see cref="PubResult"/>
        /// </summary>
        /// <param name="success"></param>
        /// <param name="resultCode"></param>
        /// <param name="error"></param>
        public PubResult(bool success, long resultCode, string error)
        {
            IsSuccess = success;
            ResultCode = resultCode;
            Message = error;
            Exception = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="resultCode"></param>
        /// <param name="error"></param>
        /// <param name="exception"></param>
        public PubResult(bool success, long resultCode, string error, Exception exception)
            : this(success, resultCode, error) => Exception = exception;
        /// <summary>
        /// 操作成功
        /// </summary>
        /// <returns></returns>
        public static PubResult Success() => new PubResult(true, (int)HttpStatusCode.OK, "操作成功");

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static PubResult Fail(string message) => new PubResult(false, (int)HttpStatusCode.InternalServerError, message);

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static PubResult Fail(Exception exception) => new PubResult(false, (int)HttpStatusCode.InternalServerError, exception.Message, exception);

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static PubResult Fail(long resultCode, string message) => new PubResult(false, resultCode, message);

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static PubResult Fail(string message, Exception exception) => Fail((int)HttpStatusCode.InternalServerError, message, exception);

        /// <summary>
        /// 操作失败
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static PubResult Fail(long resultCode, string message, Exception exception) => new PubResult(false, resultCode, message, exception);

    }
}
