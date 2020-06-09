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
    [AutoRegisterStep("Delete Resource", MsOneDriveCategory)]
    [Writable]
    public class DeleteResource:AbstractStep
    {
        [PropertyHidden]
        public override DataDescription[] InputData
        {
            get
            {
                return Concat(base.InputData, new DataDescription(typeof(string), FILE_OR_FOLDER_ID));
            }
        }
        public override OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                return Concat(base.OutcomeScenarios, new OutcomeScenarioData(DONE_OUTCOME));
            }
        }

        protected override OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data)
        {
            var fileOrFolderId = (string)data.Data[FILE_OR_FOLDER_ID];

            return OneDriveUtility.DeleteResource(connection, fileOrFolderId);
        }
    }
}
