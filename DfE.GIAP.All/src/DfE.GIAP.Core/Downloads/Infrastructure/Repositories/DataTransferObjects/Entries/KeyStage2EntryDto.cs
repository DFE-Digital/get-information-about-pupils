using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;

public class KeyStage2EntryDto
{
    [JsonProperty("ACADYR")]
    public string? ACADYR { get; set; }

    [JsonProperty("PupilMatchingRef")]
    public string? PupilMatchingRef { get; set; }

    [JsonProperty("CANDNO")]
    public string? CANDNO { get; set; }

    [JsonProperty("CAND_ID")]
    public string? CAND_ID { get; set; }

    [JsonProperty("UPN")]
    public string? UPN { get; set; }

    [JsonProperty("SURNAME")]
    public string? SURNAME { get; set; }

    [JsonProperty("FORENAMES")]
    public string? FORENAMES { get; set; }

    [JsonProperty("DOB")]
    public string? DOB { get; set; }

    [JsonProperty("YEARGRP")]
    public string? YEARGRP { get; set; }

    [JsonProperty("SEX")]
    public string? SEX { get; set; }

    [JsonProperty("LA")]
    public string? LA { get; set; }

    [JsonProperty("LA_9Code")]
    public string? LA_9Code { get; set; }

    [JsonProperty("Estab")]
    public string? ESTAB { get; set; }

    [JsonProperty("LAESTAB")]
    public string? LAESTAB { get; set; }

    [JsonProperty("ToE_CODE")]
    public string? ToE_CODE { get; set; }

    [JsonProperty("NFTYPE")]
    public string? NFTYPE { get; set; }

    [JsonProperty("MMSCH")]
    public string? MMSCH { get; set; }

    [JsonProperty("MMSCH2")]
    public string? MMSCH2 { get; set; }

    [JsonProperty("MSCH")]
    public string? MSCH { get; set; }

    [JsonProperty("MSCH2")]
    public string? MSCH2 { get; set; }

    [JsonProperty("URN")]
    public string? URN { get; set; }

    [JsonProperty("URN_AC")]
    public string? URN_AC { get; set; }

    [JsonProperty("OPEN_AC")]
    public string? OPEN_AC { get; set; }

    [JsonProperty("AMDPUPIL")]
    public string? AMDPUPIL { get; set; }

    [JsonProperty("ENDKS")]
    public string? ENDKS { get; set; }

    [JsonProperty("EXAMYEAR_RE")]
    public string? EXAMYEAR_RE { get; set; }

    [JsonProperty("EXAMYEAR_GPS")]
    public string? EXAMYEAR_GPS { get; set; }

    [JsonProperty("EXAMYEAR_MA")]
    public string? EXAMYEAR_MA { get; set; }

    [JsonProperty("SCHRES")]
    public string? SCHRES { get; set; }

    [JsonProperty("LARES")]
    public string? LARES { get; set; }

    [JsonProperty("NATRES")]
    public string? NATRES { get; set; }

    [JsonProperty("NATMTDRES")]
    public string? NATMTDRES { get; set; }

    [JsonProperty("NPDDEN_LA")]
    public string? NPDDEN_LA { get; set; }

    [JsonProperty("NPDDEN_NAT")]
    public string? NPDDEN_NAT { get; set; }

    [JsonProperty("SCHRESTA")]
    public string? SCHRESTA { get; set; }

    [JsonProperty("LARESTA")]
    public string? LARESTA { get; set; }

    [JsonProperty("NATRESTA")]
    public string? NATRESTA { get; set; }

    [JsonProperty("NATMTDRESTA")]
    public string? NATMTDRESTA { get; set; }

    [JsonProperty("EXAMYEAR_EN")]
    public string? EXAMYEAR_EN { get; set; }

    [JsonProperty("READLEVTA")]
    public string? READLEVTA { get; set; }

    [JsonProperty("WRITLEVTA")]
    public string? WRITLEVTA { get; set; }

    [JsonProperty("ENGTIER")]
    public string? ENGTIER { get; set; }

    [JsonProperty("ENGWRITMRK")]
    public string? ENGWRITMRK { get; set; }

    [JsonProperty("READLEV")]
    public string? READLEV { get; set; }

    [JsonProperty("ENGTOTMRK")]
    public string? ENGTOTMRK { get; set; }

    [JsonProperty("ENGWRITLEV")]
    public string? ENGWRITLEV { get; set; }

    [JsonProperty("ENGLEVTA")]
    public string? ENGLEVTA { get; set; }

    [JsonProperty("ENGLEV")]
    public string? ENGLEV { get; set; }

    [JsonProperty("MATTIER")]
    public string? MATTIER { get; set; }

    [JsonProperty("MATTESTAMRK")]
    public string? MATTESTAMRK { get; set; }

    [JsonProperty("MATTESTBMRK")]
    public string? MATTESTBMRK { get; set; }

    [JsonProperty("MATARTHMRK")]
    public string? MATARTHMRK { get; set; }

    [JsonProperty("MATTOTMRK")]
    public string? MATTOTMRK { get; set; }

    [JsonProperty("MATLEV")]
    public string? MATLEV { get; set; }

    [JsonProperty("MATLEVTA")]
    public string? MATLEVTA { get; set; }

    [JsonProperty("SCILEVTA")]
    public string? SCILEVTA { get; set; }

    [JsonProperty("GPSLEV")]
    public string? GPSLEV { get; set; }

    [JsonProperty("GPS_T2MARK")]
    public string? GPS_T2MARK { get; set; }

    [JsonProperty("GPS_T3MARK")]
    public string? GPS_T3MARK { get; set; }

    [JsonProperty("GPS_TESTMARK")]
    public string? GPS_TESTMARK { get; set; }

    [JsonProperty("PSENG")]
    public string? PSENG { get; set; }

    [JsonProperty("PSREAD")]
    public string? PSREAD { get; set; }

    [JsonProperty("PSWRITE")]
    public string? PSWRITE { get; set; }

    [JsonProperty("PSSPEAK")]
    public string? PSSPEAK { get; set; }

    [JsonProperty("PSLISTEN")]
    public string? PSLISTEN { get; set; }

    [JsonProperty("PSMATHS")]
    public string? PSMATHS { get; set; }

    [JsonProperty("PSNUM")]
    public string? PSNUM { get; set; }

    [JsonProperty("PSUSING")]
    public string? PSUSING { get; set; }

    [JsonProperty("PSSHAPE")]
    public string? PSSHAPE { get; set; }

    [JsonProperty("PSSCIENCE")]
    public string? PSSCIENCE { get; set; }

    [JsonProperty("KS2APSFG")]
    public string? KS2APSFG { get; set; }

    [JsonProperty("READOUTCOME")]
    public string? READOUTCOME { get; set; }

    [JsonProperty("MATOUTCOME")]
    public string? MATOUTCOME { get; set; }

    [JsonProperty("GPSOUTCOME")]
    public string? GPSOUTCOME { get; set; }

    [JsonProperty("READSCORE")]
    public string? READSCORE { get; set; }

    [JsonProperty("READSCORE_noSpeccon")]
    public string? READSCORE_noSpeccon { get; set; }

    [JsonProperty("MATSCORE")]
    public string? MATSCORE { get; set; }

    [JsonProperty("MATSCORE_noSpeccon")]
    public string? MATSCORE_noSpeccon { get; set; }

    [JsonProperty("GPSSCORE")]
    public string? GPSSCORE { get; set; }

    [JsonProperty("GPSSCORE_noSpeccon")]
    public string? GPSSCORE_noSpeccon { get; set; }

    [JsonProperty("READSPECCON")]
    public string? READSPECCON { get; set; }

    [JsonProperty("MATSPECCON")]
    public string? MATSPECCON { get; set; }

    [JsonProperty("GPSSPECCON")]
    public string? GPSSPECCON { get; set; }

    [JsonProperty("WRITTAOUTCOME")]
    public string? WRITTAOUTCOME { get; set; }

    [JsonProperty("SCITAOUTCOME")]
    public string? SCITAOUTCOME { get; set; }

    [JsonProperty("MATTAOUTCOME")]
    public string? MATTAOUTCOME { get; set; }

    [JsonProperty("READTAOUTCOME")]
    public string? READTAOUTCOME { get; set; }

    [JsonProperty("ELIGREAD")]
    public string? ELIGREAD { get; set; }

    [JsonProperty("ELIGREADTA")]
    public string? ELIGREADTA { get; set; }

    [JsonProperty("ELIGWRITTA")]
    public string? ELIGWRITTA { get; set; }

    [JsonProperty("ELIGMAT")]
    public string? ELIGMAT { get; set; }

    [JsonProperty("ELIGMATTA")]
    public string? ELIGMATTA { get; set; }

    [JsonProperty("ELIGREADLA")]
    public string? ELIGREADLA { get; set; }

    [JsonProperty("ELIGGPSLA")]
    public string? ELIGGPSLA { get; set; }

    [JsonProperty("ELIGWRITTALA")]
    public string? ELIGWRITTALA { get; set; }

    [JsonProperty("ELIGMATLA")]
    public string? ELIGMATLA { get; set; }

    [JsonProperty("ELIGRWMLA")]
    public string? ELIGRWMLA { get; set; }

    [JsonProperty("ELIGSCITA")]
    public string? ELIGSCITA { get; set; }

    [JsonProperty("ELIGGPS")]
    public string? ELIGGPS { get; set; }

    [JsonProperty("VALREAD")]
    public string? VALREAD { get; set; }

    [JsonProperty("VALREADTA")]
    public string? VALREADTA { get; set; }

    [JsonProperty("VALWRITTA")]
    public string? VALWRITTA { get; set; }

    [JsonProperty("VALMAT")]
    public string? VALMAT { get; set; }

    [JsonProperty("VALMATTA")]
    public string? VALMATTA { get; set; }

    [JsonProperty("VALSCITA")]
    public string? VALSCITA { get; set; }

    [JsonProperty("VALGPS")]
    public string? VALGPS { get; set; }

    [JsonProperty("READEXP")]
    public string? READEXP { get; set; }

    [JsonProperty("MATEXP")]
    public string? MATEXP { get; set; }

    [JsonProperty("GPSEXP")]
    public string? GPSEXP { get; set; }

    [JsonProperty("READHIGH")]
    public string? READHIGH { get; set; }

    [JsonProperty("MATHIGH")]
    public string? MATHIGH { get; set; }

    [JsonProperty("GPSHIGH")]
    public string? GPSHIGH { get; set; }

    [JsonProperty("READAT")]
    public string? READAT { get; set; }

    [JsonProperty("MATAT")]
    public string? MATAT { get; set; }

    [JsonProperty("GPSAT")]
    public string? GPSAT { get; set; }

    [JsonProperty("READMRK")]
    public string? READMRK { get; set; }

    [JsonProperty("GPSPAPER1MRK")]
    public string? GPSPAPER1MRK { get; set; }

    [JsonProperty("GPSPAPER2MRK")]
    public string? GPSPAPER2MRK { get; set; }

    [JsonProperty("GPSMRK")]
    public string? GPSMRK { get; set; }

    [JsonProperty("MATPAPER2MRK")]
    public string? MATPAPER2MRK { get; set; }

    [JsonProperty("MATPAPER3MRK")]
    public string? MATPAPER3MRK { get; set; }

    [JsonProperty("MATARITHMRK")]
    public string? MATARITHMRK { get; set; }

    [JsonProperty("MATMRK")]
    public string? MATMRK { get; set; }

    [JsonProperty("WRITTAEXP")]
    public string? WRITTAEXP { get; set; }

    [JsonProperty("SCITAEXP")]
    public string? SCITAEXP { get; set; }

    [JsonProperty("MATTAEXP")]
    public string? MATTAEXP { get; set; }

    [JsonProperty("READTAEXP")]
    public string? READTAEXP { get; set; }

    [JsonProperty("WRITTADEPTH")]
    public string? WRITTADEPTH { get; set; }

    [JsonProperty("WRITTAWTS")]
    public string? WRITTAWTS { get; set; }

    [JsonProperty("WRITTABEXP")]
    public string? WRITTABEXP { get; set; }

    [JsonProperty("SCITABEXP")]
    public string? SCITABEXP { get; set; }

    [JsonProperty("MATTABEXP")]
    public string? MATTABEXP { get; set; }

    [JsonProperty("READTABEXP")]
    public string? READTABEXP { get; set; }

    [JsonProperty("WRITTAAD")]
    public string? WRITTAAD { get; set; }

    [JsonProperty("SCITAAD")]
    public string? SCITAAD { get; set; }

    [JsonProperty("MATTAAD")]
    public string? MATTAAD { get; set; }

    [JsonProperty("READTAAD")]
    public string? READTAAD { get; set; }

    [JsonProperty("ELIGRWM")]
    public string? ELIGRWM { get; set; }

    [JsonProperty("VALRWM")]
    public string? VALRWM { get; set; }

    [JsonProperty("RWMEXP")]
    public string? RWMEXP { get; set; }

    [JsonProperty("RWMHIGH")]
    public string? RWMHIGH { get; set; }

    [JsonProperty("KS1AVERAGE")]
    public string? KS1AVERAGE { get; set; }

    [JsonProperty("INREADPROG")]
    public string? INREADPROG { get; set; }

    [JsonProperty("INWRITPROG")]
    public string? INWRITPROG { get; set; }

    [JsonProperty("INMATPROG")]
    public string? INMATPROG { get; set; }

    [JsonProperty("KS1AVERAGE_GRP")]
    public string? KS1AVERAGE_GRP { get; set; }

    [JsonProperty("KS1AVERAGE_GRP_P")]
    public string? KS1AVERAGE_GRP_P { get; set; }

    [JsonProperty("KS2READSCORE")]
    public string? KS2READSCORE { get; set; }

    [JsonProperty("KS2WRITSCORE")]
    public string? KS2WRITSCORE { get; set; }

    [JsonProperty("KS2MATSCORE")]
    public string? KS2MATSCORE { get; set; }

    [JsonProperty("KS2READPRED")]
    public string? KS2READPRED { get; set; }

    [JsonProperty("KS2WRITPRED")]
    public string? KS2WRITPRED { get; set; }

    [JsonProperty("KS2MATPRED")]
    public string? KS2MATPRED { get; set; }

    [JsonProperty("KS2READPRED_P")]
    public string? KS2READPRED_P { get; set; }

    [JsonProperty("KS2WRITPRED_P")]
    public string? KS2WRITPRED_P { get; set; }

    [JsonProperty("KS2MATPRED_P")]
    public string? KS2MATPRED_P { get; set; }

    [JsonProperty("READPROGSCORE")]
    public string? READPROGSCORE { get; set; }

    [JsonProperty("WRITPROGSCORE")]
    public string? WRITPROGSCORE { get; set; }

    [JsonProperty("MATPROGSCORE")]
    public string? MATPROGSCORE { get; set; }

    [JsonProperty("READPROGSCORE_P")]
    public string? READPROGSCORE_P { get; set; }

    [JsonProperty("WRITPROGSCORE_P")]
    public string? WRITPROGSCORE_P { get; set; }

    [JsonProperty("MATPROGSCORE_P")]
    public string? MATPROGSCORE_P { get; set; }

    [JsonProperty("READPROGSCORE_P_ADJUSTED")]
    public string? READPROGSCORE_P_ADJUSTED { get; set; }

    [JsonProperty("WRITPROGSCORE_P_ADJUSTED")]
    public string? WRITPROGSCORE_P_ADJUSTED { get; set; }

    [JsonProperty("MATPROGSCORE_P_ADJUSTED")]
    public string? MATPROGSCORE_P_ADJUSTED { get; set; }

    [JsonProperty("KS1READPS")]
    public string? KS1READPS { get; set; }

    [JsonProperty("KS1WRITPS")]
    public string? KS1WRITPS { get; set; }

    [JsonProperty("KS1MATPS")]
    public string? KS1MATPS { get; set; }

    [JsonProperty("EALGRP")]
    public string? EALGRP { get; set; }

    [JsonProperty("CLA_PP_1_DAY")]
    public string? CLA_PP_1_DAY { get; set; }

    [JsonProperty("CLA_6_MONTHS")]
    public string? CLA_6_MONTHS { get; set; }

    [JsonProperty("CLA_12_MONTHS")]
    public string? CLA_12_MONTHS { get; set; }

    [JsonProperty("CLA_PP_6_MONTHS")]
    public string? CLA_PP_6_MONTHS { get; set; }

    [JsonProperty("SENTYPE")]
    public string? SENTYPE { get; set; }

    [JsonProperty("KS1GROUP")]
    public string? KS1GROUP { get; set; }

    [JsonProperty("FSM")]
    public string? FSM { get; set; }

    [JsonProperty("FSM6")]
    public string? FSM6 { get; set; }

    [JsonProperty("FSM6_P")]
    public string? FSM6_P { get; set; }

    [JsonProperty("NEWMOBILE")]
    public string? NEWMOBILE { get; set; }

    [JsonProperty("KS2READPRED_EM")]
    public string? KS2READPRED_EM { get; set; }

    [JsonProperty("KS2WRITPRED_EM")]
    public string? KS2WRITPRED_EM { get; set; }

    [JsonProperty("KS2MATPRED_EM")]
    public string? KS2MATPRED_EM { get; set; }

    [JsonProperty("READPROGSCORE_EM")]
    public string? READPROGSCORE_EM { get; set; }

    [JsonProperty("WRITPROGSCORE_EM")]
    public string? WRITPROGSCORE_EM { get; set; }

    [JsonProperty("MATPROGSCORE_EM")]
    public string? MATPROGSCORE_EM { get; set; }

    [JsonProperty("READPROGSCORE_EM_ADJUSTED")]
    public string? READPROGSCORE_EM_ADJUSTED { get; set; }

    [JsonProperty("WRITPROGSCORE_EM_ADJUSTED")]
    public string? WRITPROGSCORE_EM_ADJUSTED { get; set; }

    [JsonProperty("MATPROGSCORE_EM_ADJUSTED")]
    public string? MATPROGSCORE_EM_ADJUSTED { get; set; }

    [JsonProperty("READEXP_noSpeccon")]
    public string? READEXP_noSpeccon { get; set; }

    [JsonProperty("MATEXP_noSpeccon")]
    public string? MATEXP_noSpeccon { get; set; }

    [JsonProperty("GPSEXP_noSpeccon")]
    public string? GPSEXP_noSpeccon { get; set; }

    [JsonProperty("READHIGH_noSpeccon")]
    public string? READHIGH_noSpeccon { get; set; }

    [JsonProperty("MATHIGH_noSpeccon")]
    public string? MATHIGH_noSpeccon { get; set; }

    [JsonProperty("GPSHIGH_noSpeccon")]
    public string? GPSHIGH_noSpeccon { get; set; }

    [JsonProperty("RWMEXP_noSpeccon")]
    public string? RWMEXP_noSpeccon { get; set; }

    [JsonProperty("RWMHIGH_noSpeccon")]
    public string? RWMHIGH_noSpeccon { get; set; }

    [JsonProperty("INREADPROG_noSpeccon")]
    public string? INREADPROG_noSpeccon { get; set; }

    [JsonProperty("INMATPROG_noSpeccon")]
    public string? INMATPROG_noSpeccon { get; set; }

    [JsonProperty("KS2READSCORE_noSpeccon")]
    public string? KS2READSCORE_noSpeccon { get; set; }

    [JsonProperty("KS2MATSCORE_noSpeccon")]
    public string? KS2MATSCORE_noSpeccon { get; set; }

    [JsonProperty("KS2READPRED_EM_noSpeccon")]
    public string? KS2READPRED_EM_noSpeccon { get; set; }

    [JsonProperty("KS2MATPRED_EM_noSpeccon")]
    public string? KS2MATPRED_EM_noSpeccon { get; set; }

    [JsonProperty("READPROGSCORE_EM_noSpeccon")]
    public string? READPROGSCORE_EM_noSpeccon { get; set; }

    [JsonProperty("MATPROGSCORE_EM_noSpeccon")]
    public string? MATPROGSCORE_EM_noSpeccon { get; set; }

    [JsonProperty("READPROGSCORE_EM_ADJUSTED_noSpeccon")]
    public string? READPROGSCORE_EM_ADJUSTED_noSpeccon { get; set; }

    [JsonProperty("MATPROGSCORE_EM_ADJUSTED_noSpeccon")]
    public string? MATPROGSCORE_EM_ADJUSTED_noSpeccon { get; set; }

    [JsonProperty("VERSION")]
    public string? VERSION { get; set; }
}
