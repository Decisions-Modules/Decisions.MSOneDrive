using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;

namespace Decisions.MSOneDrive
{
    [AutoRegisterStep("Create Folder", MsOneDriveCategory)]
    [Writable]
    class CreateFolder : AbstractStep
    {
        [PropertyHidden]
        public override DataDescription[] InputData
        {
            get
            {
                var data = new DataDescription[] { new DataDescription(typeof(string), PARENT_FOLDER_ID), new DataDescription(typeof(string), NEW_FOLDER_NAME) };
                return base.InputData.Concat(data).ToArray();
            }
        }
        public override OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                var data = new OutcomeScenarioData[] { new OutcomeScenarioData(RESULT_OUTCOME, new DataDescription(typeof(OneDriveFolder), RESULT)) };
                return base.OutcomeScenarios.Concat(data).ToArray();
            }
        }

        protected override OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data)
        {
            var parentFolderId = (string)data.Data[PARENT_FOLDER_ID];
            var newFolderName = (string)data.Data[NEW_FOLDER_NAME];

            return OneDriveUtility.CreateFolder(connection, newFolderName, parentFolderId);
        }
    }
}
