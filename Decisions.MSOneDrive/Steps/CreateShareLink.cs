using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using Microsoft.Graph;
using System;

namespace Decisions.MSOneDrive
{
    [AutoRegisterStep("Create Sharing Link", MsOneDriveCategory)]
    [Writable]
    class CreateShareLink : AbstractStep
    {
        [PropertyHidden]
        public override DataDescription[] InputData
        {
            get
            {
                return Concat(base.InputData,
                    new DataDescription(typeof(string), FILE_OR_FOLDER_ID),
                    new DataDescription(typeof(OneDriveShareType), TYPE_OF_LINK),
                    new DataDescription(typeof(OneDriveShareScope), SCOPE_OF_LINK));
            }
        }

        public override OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                return Concat(base.OutcomeScenarios, new OutcomeScenarioData(RESULT_OUTCOME, new DataDescription(typeof(OneDrivePermission), RESULT)));
            }
        }

        protected override OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data)
        {
            var folderId = (string)data.Data[FILE_OR_FOLDER_ID];
            OneDriveShareType shareType = (OneDriveShareType)data.Data[TYPE_OF_LINK];
            OneDriveShareScope shareScope = (OneDriveShareScope)data.Data[SCOPE_OF_LINK];
            return OneDriveUtility.CreateShareLink(connection, folderId, shareType, shareScope);
        }


    }
}
