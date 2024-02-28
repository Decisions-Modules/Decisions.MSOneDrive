using System;
using System.Collections.Generic;
using System.ComponentModel;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.Flow.Mapping;
using DecisionsFramework.Design.Properties;
using Microsoft.Graph;
using System.Linq;
using System.Runtime.CompilerServices;
using DecisionsFramework.Data.DataTypes;
using DecisionsFramework.ServiceLayer.Services.FileReference;

namespace Decisions.MSOneDrive
{
    public enum TypeToUpload
    {
        [Description("File Reference")]
        FILE_REFERENCE,
        [Description("File Data")]
        FILE_DATA
    }
    
    [AutoRegisterStep("Upload File Data or File Reference", MsOneDriveCategory)]
    [Writable]
    public class UploadFileDataOrFileReference : AbstractStep, INotifyPropertyChanged
    {
        [WritableValue]
        private TypeToUpload _typeToUpload = TypeToUpload.FILE_DATA;
        
        [PropertyClassification(1, "Type To Upload", "Settings")]
        public TypeToUpload TypeToUpload
        {
            get => _typeToUpload;
            set
            {
                _typeToUpload = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InputData));
            }
        }
        
        [PropertyHidden]
        public override DataDescription[] InputData
        {
            get
            {
                List<DataDescription> inputs = new List<DataDescription>
                {
                    new DataDescription(typeof(string), PARENT_FOLDER_ID)
                };

                switch (_typeToUpload)
                {
                    case TypeToUpload.FILE_REFERENCE:
                        inputs.Add(new DataDescription(typeof(FileReference), FILE_REFERENCE));
                        break;
                    case TypeToUpload.FILE_DATA:
                        inputs.Add(new DataDescription(typeof(FileData), FILE_DATA));
                        break;
                }
                
                return base.InputData.Concat(inputs).ToArray();
            }
        }
        public override OutcomeScenarioData[] OutcomeScenarios
        {
            get
            {
                var data = new OutcomeScenarioData[] { new OutcomeScenarioData(RESULT_OUTCOME, new DataDescription(typeof(OneDriveFile), RESULT)) };
                return base.OutcomeScenarios.Concat(data).ToArray();
            }
        }

        protected override OneDriveBaseResult ExecuteStep(GraphServiceClient connection, StepStartData data)
        {
            string folderId = (string)data.Data[PARENT_FOLDER_ID];
            string filePath = String.Empty;
            
            switch (_typeToUpload)
            {
                case TypeToUpload.FILE_REFERENCE:
                    FileReference fileReference = (FileReference)data.Data[FILE_REFERENCE];
                    filePath = FileReferenceService.GetFilePathFromIdOrName(fileReference.Id);
                    break;
                case TypeToUpload.FILE_DATA:
                    FileData filedata = (FileData)data.Data[FILE_DATA];
                    filePath = FileReferenceService.GetFilePathFromIdOrName(filedata.Id);
                    break;
            }
            
            return OneDriveUtility.UploadFile(connection, filePath, null, folderId, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
