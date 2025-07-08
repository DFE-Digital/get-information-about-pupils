using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Common.Validation
{
    public static class DownloadValidation
    {

        public static bool ValidateDownloadDataType(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
        {
            switch (type)
            {
                case DownloadDataType.EYFSP:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 10 &&
                            statutoryHighAge >= 3 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                case DownloadDataType.KS1:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 13 &&
                            statutoryHighAge >= 6 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                case DownloadDataType.KS2:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 15 &&
                            statutoryHighAge >= 6 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                case DownloadDataType.KS4:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 17 &&
                            statutoryHighAge >= 12 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                case DownloadDataType.Phonics:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 10 &&
                            statutoryHighAge >= 3 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                case DownloadDataType.MTC:
                    return (statutoryLowAge >= 2 && statutoryLowAge <= 14 &&
                            statutoryHighAge >= 4 && statutoryHighAge <= 25 &&
                            statutoryLowAge < statutoryHighAge);
                default:
                    return false;
            }
        }

        public static bool ValidateUlnDownloadDataType(DownloadUlnDataType type, int statutoryHighAge)
        {
            switch (type)
            {
                case DownloadUlnDataType.PP:
                    return statutoryHighAge >= 14;
                case DownloadUlnDataType.SEN:
                    return statutoryHighAge >= 14;
                default:
                    return false;
            }
        }
    }
}
