using Decisions.OAuth;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using DecisionsFramework.Design.Properties.Attributes;
using DecisionsFramework.ServiceLayer.Services.ContextData;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    [Writable]
    public abstract class AbstractStep : ISyncStep, IDataConsumer, IDataProducer
    {
        public const string MsOneDriveCategory = "Integration/MS OneDrive";

        protected const string ERROR_OUTCOME = "Error";
        protected const string RESULT_OUTCOME = "Result";
        protected const string DONE_OUTCOME = "Done";
        protected const string ERROR_OUTCOME_DATA_NAME = "Error info";
        protected const string RESULT = "RESULT";

        protected const string FILE_OR_FOLDER_ID = "File Or Folder Id";
        protected const string FILE_ID = "File Id";
        protected const string PERMISSION_ID = "Permission Id";
        protected const string PARENT_FOLDER_ID = "Parent Folder Id";
        protected const string LOCAL_FILE_PATH = "Local File Path";
        protected const string PERMISSION = "Permission";
        protected const string NEW_FOLDER_NAME = "New Folder Name";

        protected const string TYPE_OF_LINK = "Type Of Link";
        protected const string SCOPE_OF_LINK = "Scope of Link";

        /*OAuth2TokenResponse resp;
        OAuthToken token*/

        protected static T[] Concat<T>(T[] array, params T[] newItems)
        {
            var res = new List<T>(array);
            res.AddRange(newItems);
            T[] resArr = res.ToArray();
            return resArr;
        }

        [PropertyHidden]
        public virtual DataDescription[] InputData
        {
            get
            {
                return new DataDescription[] { };
            }
        }

        private const int ERROR_OUTCOME_INDEX = 0;
        private const int RESULT_OUTCOME_INDEX = 1;
        public virtual OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                return new OutcomeScenarioData[] { new OutcomeScenarioData(ERROR_OUTCOME, new DataDescription(typeof(OneDriveErrorInfo), ERROR_OUTCOME_DATA_NAME)) };
            }
        }

        [EntityPickerEditor(new Type[] { typeof(OAuthToken) }, "MS OneDrive Token")]
        //[TokenPicker]
        public string AccessToken { get; set; }

        public ResultData Run(StepStartData data)
        {
            try
            {
                GraphServiceClient connection = AuthenticationHelper.GetAuthenticatedClient(AccessToken);
                OneDriveBaseResult res = ExecuteStep(connection, data);

                if (res.IsSucceed)
                {
                    var outputData = OutcomeScenarios[RESULT_OUTCOME_INDEX].OutputData;
                    var exitPointName = OutcomeScenarios[RESULT_OUTCOME_INDEX].ExitPointName;

                    if (outputData != null && outputData.Length > 0)
                        return new ResultData(exitPointName, new DataPair[] { new DataPair(outputData[0].Name, res.DataObj) });
                    else
                        return new ResultData(exitPointName);
                }
                else
                {
                    return new ResultData(ERROR_OUTCOME, new DataPair[] { new DataPair(ERROR_OUTCOME_DATA_NAME, res.ErrorInfo) });
                }
            }
            catch (Exception ex)
            {
                OneDriveErrorInfo ErrInfo = new OneDriveErrorInfo() { ErrorMessage = ex.ToString(), HttpErrorCode = null };
                return new ResultData(ERROR_OUTCOME, new DataPair[] { new DataPair(ERROR_OUTCOME_DATA_NAME, ErrInfo) });
                //throw new LoggedException("Error running step", ex);
            }
        }

        protected abstract OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data);

    }
}

