using Decisions.OAuth;
using DecisionsFramework.Data.ORMapper;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using DecisionsFramework.Design.Properties.Attributes;
using DecisionsFramework.ServiceLayer.Services.ContextData;
using Microsoft.Graph;
using System;

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
        protected const string FILE_REFERENCE = "File Reference";
        protected const string FILE_DATA = "File Data";
        protected const string PERMISSION = "Permission";
        protected const string NEW_FOLDER_NAME = "New Folder Name";

        protected const string TYPE_OF_LINK = "Type Of Link";
        protected const string SCOPE_OF_LINK = "Scope of Link";

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

        [TokenPicker]
        [WritableValue]
        [PropertyClassification(0, "Token", "Settings")]
        public string Token { get; set; }

        private string FindAccessToken(string id)
        {
            ORM<OAuthToken> orm = new ORM<OAuthToken>();
            var token = orm.Fetch(id);
            if (token != null)
                return token.TokenData;
            throw new EntityNotFoundException($"Can not find token with TokenId=\"{id}\"");
        }
        public ResultData Run(StepStartData data)
        {
            try
            {
                var accessToken = FindAccessToken(Token);
                GraphServiceClient connection = AuthenticationHelper.GetAuthenticatedClient(accessToken);
                OneDriveBaseResult res = ExecuteStep(connection, data);
                
                if (res.IsSucceed)
                {
                    var outputData = OutcomeScenarios[RESULT_OUTCOME_INDEX].OutputData;
                    var exitPointName = OutcomeScenarios[RESULT_OUTCOME_INDEX].ExitPointName;

                    if (outputData != null && outputData.Length > 0)
                        return new ResultData(exitPointName, new DataPair[] { new DataPair(outputData[0].Name, res.DataObj) });
                    
                    return new ResultData(exitPointName);
                }

                return new ResultData(ERROR_OUTCOME, new DataPair[] { new DataPair(ERROR_OUTCOME_DATA_NAME, res.ErrorInfo) });
            }
            catch (Exception ex)
            {
                OneDriveErrorInfo errInfo = new OneDriveErrorInfo()
                {
                    ErrorMessage = ex.ToString(), 
                    HttpErrorCode = null
                };
                
                return new ResultData(ERROR_OUTCOME, new DataPair[] { new DataPair(ERROR_OUTCOME_DATA_NAME, errInfo) });
            }
        }

        protected abstract OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data);

    }
}

