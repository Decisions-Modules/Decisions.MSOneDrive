using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    [AutoRegisterStep("Upload File", MsOneDriveCategory)]
    [Writable]
    public class UploadFile : AbstractStep
    {
        [PropertyHidden]
        public override DataDescription[] InputData
        {
            get
            {
                return Concat(base.InputData, new DataDescription(typeof(string), PARENT_FOLDER_ID), new DataDescription(typeof(string), LOCAL_FILE_PATH));
            }
        }
        public override OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                return Concat(base.OutcomeScenarios, new OutcomeScenarioData(RESULT_OUTCOME, new DataDescription(typeof(OneDriveFile), RESULT)));
            }
        }

        protected override OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data)
        {
            var folderId = (string)data.Data[PARENT_FOLDER_ID];
            var localFilePath = (string)data.Data[LOCAL_FILE_PATH];
            var fileName = System.IO.Path.GetFileName(localFilePath);

            return OneDriveUtility.UploadFile(connection, localFilePath, fileName, folderId);
        }
    }
}
