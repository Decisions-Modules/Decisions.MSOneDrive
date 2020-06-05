using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    [DataContract]
    public class GoogleDriveErrorInfo
    {
        [DataMember] public string ErrorMessage { get; set; }
        [DataMember] public HttpStatusCode? HttpErrorCode { get; set; }
    }

    public class OneDriveBaseResult
    {
        public bool IsSucceed;
        public GoogleDriveErrorInfo ErrorInfo = new GoogleDriveErrorInfo();
        public virtual object DataObj { get { return null; } }

        public bool FillFromException(Exception exception)
        {
            IsSucceed = false;
            ErrorInfo = new GoogleDriveErrorInfo();
            if (exception is AggregateException)
            { 
                var ex = (AggregateException)exception;
                if (ex.InnerExceptions.Count==1 && ex.InnerException is ServiceException)
                    exception = ex.InnerException;
            };

            if (exception is ServiceException)
            {
                var ex = (ServiceException)exception;
                ErrorInfo.ErrorMessage = (ex.Message ?? ex.ToString());
                ErrorInfo.HttpErrorCode = ex.StatusCode;
                return true;
            }

            return false;
        }
    }

    public class OneDriveResultWithData<T> : OneDriveBaseResult
    {
        public T Data { get; set; }
        public override object DataObj { get { return Data; } }

        public OneDriveResultWithData() { }

        internal OneDriveResultWithData(OneDriveBaseResult baseResult)
        {
            ErrorInfo = baseResult.ErrorInfo;
            IsSucceed = baseResult.IsSucceed;
        }

    }
}
