using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;

public class KeyStage4EntryDto
{
    [JsonProperty("KS4_ACADYR")]
    public string? ACADYR { get; set; }

    [JsonProperty("KS4_PUPILMATCHINGREF")]
    public string? PUPILMATCHINGREF { get; set; }

    [JsonProperty("KS4_UPN")]
    public string? UPN { get; set; }

    [JsonProperty("KS4_SURNAME")]
    public string? SURNAME { get; set; }

    [JsonProperty("KS4_FORENAMES")]
    public string? FORENAMES { get; set; }

    [JsonProperty("KS4_DOB")]
    public string? DOB { get; set; }

    [JsonProperty("KS4_YEARGRP")]
    public string? YEARGRP { get; set; }

    [JsonProperty("KS4_ACTYRGRP")]
    public string? ACTYRGRP { get; set; }

    //ToDo: look to see if we can clean out the unused items
    [JsonProperty("KS4_GENDER")]
    public string? GENDER { get; set; }

    [JsonProperty("KS4_SEX")]
    public string? SEX { get; set; }

    [JsonProperty("KS4_IDACI")]
    public string? IDACI { get; set; }

    [JsonProperty("KS4_LA")]
    public string? LA { get; set; }

    [JsonProperty("KS4_LA_9CODE")]
    public string? LA_9CODE { get; set; }

    [JsonProperty("KS4_ESTAB")]
    public string? ESTAB { get; set; }

    [JsonProperty("KS4_LAESTAB")]
    public string? LAESTAB { get; set; }

    [JsonProperty("KS4_PDFECN")]
    public string? PDFECN { get; set; }

    [JsonProperty("KS4_URN")]
    public string? URN { get; set; }

    [JsonProperty("KS4_URN_AC")]
    public string? URN_AC { get; set; }

    [JsonProperty("KS4_OPEN_AC")]
    public string? OPEN_AC { get; set; }

    [JsonProperty("KS4_TOE_CODE")]
    public string? TOE_CODE { get; set; }

    [JsonProperty("KS4_NFTYPE")]
    public string? NFTYPE { get; set; }

    [JsonProperty("KS4_NEW_TYPE")]
    public string? NEW_TYPE { get; set; }

    [JsonProperty("KS4_NEWER_TYPE")]
    public string? NEWER_TYPE { get; set; }

    [JsonProperty("KS4_MMSCH")]
    public string? MMSCH { get; set; }

    [JsonProperty("KS4_MMSCH2")]
    public string? MMSCH2 { get; set; }

    [JsonProperty("KS4_MSCH")]
    public string? MSCH { get; set; }

    [JsonProperty("KS4_MSCH2")]
    public string? MSCH2 { get; set; }

    [JsonProperty("KS4_NCN")]
    public string? NCN { get; set; }

    [JsonProperty("KS4_ENTRIES")]
    public string? ENTRIES { get; set; }

    [JsonProperty("KS4_AMDFLAG")]
    public string? AMDFLAG { get; set; }

    [JsonProperty("KS4_ENDKS")]
    public string? ENDKS { get; set; }

    [JsonProperty("KS4_EARLYT_E")]
    public string? EARLYT_E { get; set; }

    [JsonProperty("KS4_NORFLAGE")]
    public string? NORFLAGE { get; set; }

    [JsonProperty("KS4_SCHRES")]
    public string? SCHRES { get; set; }

    [JsonProperty("KS4_LARES")]
    public string? LARES { get; set; }

    [JsonProperty("KS4_NATRES")]
    public string? NATRES { get; set; }

    [JsonProperty("KS4_NATMTDRES")]
    public string? NATMTDRES { get; set; }

    [JsonProperty("KS4_SCHNOR")]
    public string? SCHNOR { get; set; }

    [JsonProperty("KS4_LANOR")]
    public string? LANOR { get; set; }

    [JsonProperty("KS4_NATNOR")]
    public string? NATNOR { get; set; }

    [JsonProperty("KS4_NPDDEN_NAT")]
    public string? NPDDEN_NAT { get; set; }

    [JsonProperty("KS4_NPDNUM_NAT")]
    public string? NPDNUM_NAT { get; set; }

    [JsonProperty("KS4_NPDDEN_LA")]
    public string? NPDDEN_LA { get; set; }

    [JsonProperty("KS4_NPDNUM_LA")]
    public string? NPDNUM_LA { get; set; }

    [JsonProperty("KS4_ALLSCI_PTQ_EE")]
    public string? ALLSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENT1GMFL_91")]
    public string? ENT1GMFL_91 { get; set; }

    [JsonProperty("KS4_ENT1EMFL_PTQ_EE")]
    public string? ENT1EMFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENTRY_G_PTQ_EE")]
    public string? ENTRY_G_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENTRY_E_3NG_PTQ_EE")]
    public string? ENTRY_E_3NG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PTSPE_PTQ_EE")]
    public string? PTSPE_PTQ_EE { get; set; }

    [JsonProperty("KS4_GPTSPE_PTQ_EE")]
    public string? GPTSPE_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENTRY_1_3NG_PTQ_EE")]
    public string? ENTRY_1_3NG_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENTRY_5_3NG_PTQ_EE")]
    public string? ENTRY_5_3NG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EXAMCAT_PTQ_EE")]
    public string? EXAMCAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_94")]
    public string? PASS_94 { get; set; }

    [JsonProperty("KS4_PASS_1_94")]
    public string? PASS_1_94 { get; set; }

    [JsonProperty("KS4_PASS_LEV2_PTQ_EE")]
    public string? PASS_LEV2_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_LEV2EM_PTQ_EE")]
    public string? PASS_LEV2EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_LEV1_PTQ_EE")]
    public string? PASS_LEV1_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_LEV1EM_PTQ_EE")]
    public string? PASS_LEV1EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_BTEC_PTQ_EE")]
    public string? BTEC_PTQ_EE { get; set; }

    [JsonProperty("KS4_PTSTNEWE_PTQ_EE")]
    public string? PTSTNEWE_PTQ_EE { get; set; }

    [JsonProperty("KS4_PTSCNEWE_PTQ_EE")]
    public string? PTSCNEWE_PTQ_EE { get; set; }

    [JsonProperty("KS4_AVPTSENT_PTQ_EE")]
    public string? AVPTSENT_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEVEL2_PTQ_EE")]
    public string? LEVEL2_PTQ_EE { get; set; }

    [JsonProperty("KS4_GLEVEL2_PTQ_EE")]
    public string? GLEVEL2_PTQ_EE { get; set; }

    [JsonProperty("KS4_GLEVEL2EM_PTQ_EE")]
    public string? GLEVEL2EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEVEL1_PTQ_EE")]
    public string? LEVEL1_PTQ_EE { get; set; }

    [JsonProperty("KS4_ANYLEV1_PTQ_EE")]
    public string? ANYLEV1_PTQ_EE { get; set; }

    [JsonProperty("KS4_ANYPASS_PTQ_EE")]
    public string? ANYPASS_PTQ_EE { get; set; }

    [JsonProperty("KS4_VOCQUAL_PTQ_EE")]
    public string? VOCQUAL_PTQ_EE { get; set; }

    [JsonProperty("KS4_HGMATH_91")]
    public string? HGMATH_91 { get; set; }

    [JsonProperty("KS4_KS4SCI_91")]
    public string? KS4SCI_91 { get; set; }

    [JsonProperty("KS4_LEVEL2_EM_94")]
    public string? LEVEL2_EM_94 { get; set; }

    [JsonProperty("KS4_LEVEL1_EM_PTQ_EE")]
    public string? LEVEL1_EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2EM_94")]
    public string? LEV2EM_94 { get; set; }

    [JsonProperty("KS4_LEV2EM_95")]
    public string? LEV2EM_95 { get; set; }

    [JsonProperty("KS4_LEV1EM_PTQ_EE")]
    public string? LEV1EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIA_94")]
    public string? LEV2SCIA_94 { get; set; }

    [JsonProperty("KS4_LEV2SCIA_95")]
    public string? LEV2SCIA_95 { get; set; }

    [JsonProperty("KS4_LEV2SCIC_94")]
    public string? LEV2SCIC_94 { get; set; }

    [JsonProperty("KS4_LEV2SCIC_95")]
    public string? LEV2SCIC_95 { get; set; }

    [JsonProperty("KS4_LEV2SCID_94")]
    public string? LEV2SCID_94 { get; set; }

    [JsonProperty("KS4_LEV2SCID_95")]
    public string? LEV2SCID_95 { get; set; }

    [JsonProperty("KS4_LEV2SCI2_94")]
    public string? LEV2SCI2_94 { get; set; }

    [JsonProperty("KS4_LEV2SCI2_95")]
    public string? LEV2SCI2_95 { get; set; }

    [JsonProperty("KS4_LEVEL2MFL_94")]
    public string? LEVEL2MFL_94 { get; set; }

    [JsonProperty("KS4_LEVEL2MFL_95")]
    public string? LEVEL2MFL_95 { get; set; }

    [JsonProperty("KS4_LEVEL1MFL_PTQ_EE")]
    public string? LEVEL1MFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_ANYPMFL_PTQ_EE")]
    public string? ANYPMFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_ENGATT_PTQ_EE")]
    public string? GCSE_ENGATT_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_ENG_91")]
    public string? GCSE_ENG_91 { get; set; }

    [JsonProperty("KS4_GCSE_ENG_94")]
    public string? GCSE_ENG_94 { get; set; }

    [JsonProperty("KS4_GCSE_ENG_95")]
    public string? GCSE_ENG_95 { get; set; }

    [JsonProperty("KS4_GCSE_MATHATT_PTQ_EE")]
    public string? GCSE_MATHATT_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_MATH_91")]
    public string? GCSE_MATH_91 { get; set; }

    [JsonProperty("KS4_GCSE_MATH_94")]
    public string? GCSE_MATH_94 { get; set; }

    [JsonProperty("KS4_GCSE_MATH_95")]
    public string? GCSE_MATH_95 { get; set; }

    [JsonProperty("KS4_GCSE_SCIATT_PTQ_EE")]
    public string? GCSE_SCIATT_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_SCI_91")]
    public string? GCSE_SCI_91 { get; set; }

    [JsonProperty("KS4_GCSE_SCI_94")]
    public string? GCSE_SCI_94 { get; set; }

    [JsonProperty("KS4_EBACENG_94")]
    public string? EBACENG_94 { get; set; }

    [JsonProperty("KS4_EBACENG_95")]
    public string? EBACENG_95 { get; set; }

    [JsonProperty("KS4_EBALLSCI_PTQ_EE")]
    public string? EBALLSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS2SCIA_94")]
    public string? PASS2SCIA_94 { get; set; }

    [JsonProperty("KS4_PASS2SCIA_95")]
    public string? PASS2SCIA_95 { get; set; }

    [JsonProperty("KS4_PASS2SCIB_94")]
    public string? PASS2SCIB_94 { get; set; }

    [JsonProperty("KS4_PASS2SCIB_95")]
    public string? PASS2SCIB_95 { get; set; }

    [JsonProperty("KS4_PASSCOMBSCI_94")]
    public string? PASSCOMBSCI_94 { get; set; }

    [JsonProperty("KS4_PASSCOMBSCI_95")]
    public string? PASSCOMBSCI_95 { get; set; }

    [JsonProperty("KS4_PASSCOMBSCI_91")]
    public string? PASSCOMBSCI_91 { get; set; }

    [JsonProperty("KS4_EBAC2SCI_94")]
    public string? EBAC2SCI_94 { get; set; }

    [JsonProperty("KS4_EBAC2SCI_95")]
    public string? EBAC2SCI_95 { get; set; }

    [JsonProperty("KS4_EBACMAT_94")]
    public string? EBACMAT_94 { get; set; }

    [JsonProperty("KS4_EBACMAT_95")]
    public string? EBACMAT_95 { get; set; }

    [JsonProperty("KS4_EBACHUM_94")]
    public string? EBACHUM_94 { get; set; }

    [JsonProperty("KS4_EBACHUM_95")]
    public string? EBACHUM_95 { get; set; }

    [JsonProperty("KS4_EBACLAN_94")]
    public string? EBACLAN_94 { get; set; }

    [JsonProperty("KS4_EBACLAN_95")]
    public string? EBACLAN_95 { get; set; }

    [JsonProperty("KS4_EBACC_94")]
    public string? EBACC_94 { get; set; }

    [JsonProperty("KS4_EBACC_95")]
    public string? EBACC_95 { get; set; }

    [JsonProperty("KS4_EBACCAPS_PUPIL")]
    public string? EBACCAPS_PUPIL { get; set; }

    [JsonProperty("KS4_L2BASICS_94")]
    public string? L2BASICS_94 { get; set; }

    [JsonProperty("KS4_L2BASICS_95")]
    public string? L2BASICS_95 { get; set; }

    [JsonProperty("KS4_EBACENG_E_PTQ_EE")]
    public string? EBACENG_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACMAT_E_PTQ_EE")]
    public string? EBACMAT_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBAC2SCI_E_PTQ_EE")]
    public string? EBAC2SCI_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACHUM_E_PTQ_EE")]
    public string? EBACHUM_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACLAN_E_PTQ_EE")]
    public string? EBACLAN_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACC_E_PTQ_EE")]
    public string? EBACC_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACENG91")]
    public string? EBACENG91 { get; set; }

    [JsonProperty("KS4_PASS2SCIA_91")]
    public string? PASS2SCIA_91 { get; set; }

    [JsonProperty("KS4_PASS2SCIB_91")]
    public string? PASS2SCIB_91 { get; set; }

    [JsonProperty("KS4_EBAC2SCI_91")]
    public string? EBAC2SCI_91 { get; set; }

    [JsonProperty("KS4_EBACMAT91")]
    public string? EBACMAT91 { get; set; }

    [JsonProperty("KS4_EBACHUM_91")]
    public string? EBACHUM_91 { get; set; }

    [JsonProperty("KS4_EBACLAN_91")]
    public string? EBACLAN_91 { get; set; }

    [JsonProperty("KS4_EBACC91")]
    public string? EBACC91 { get; set; }

    [JsonProperty("KS4_INVAMOD_PTQ_EE")]
    public string? INVAMOD_PTQ_EE { get; set; }

    [JsonProperty("KS4_INSCIVAMOD_PTQ_EE")]
    public string? INSCIVAMOD_PTQ_EE { get; set; }

    [JsonProperty("KS4_INHUMVAMOD_PTQ_EE")]
    public string? INHUMVAMOD_PTQ_EE { get; set; }

    [JsonProperty("KS4_INLANVAMOD_PTQ_EE")]
    public string? INLANVAMOD_PTQ_EE { get; set; }

    [JsonProperty("KS4_INVACALC_PTQ_EE")]
    public string? INVACALC_PTQ_EE { get; set; }

    [JsonProperty("KS4_INSCIVACALC_PTQ_EE")]
    public string? INSCIVACALC_PTQ_EE { get; set; }

    [JsonProperty("KS4_INHUMVACALC_PTQ_EE")]
    public string? INHUMVACALC_PTQ_EE { get; set; }

    [JsonProperty("KS4_INLANVACALC_PTQ_EE")]
    public string? INLANVACALC_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TAENG_PTQ_EE")]
    public string? VAP2TAENG_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TAMAT_PTQ_EE")]
    public string? VAP2TAMAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TAAPS_PTQ_EE")]
    public string? VAP2TAAPS_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TAAPSSQ_PTQ_EE")]
    public string? VAP2TAAPSSQ_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TAAPSCU_PTQ_EE")]
    public string? VAP2TAAPSCU_PTQ_EE { get; set; }

    [JsonProperty("KS4_P2TAE_DEV_PTQ_EE")]
    public string? P2TAE_DEV_PTQ_EE { get; set; }

    [JsonProperty("KS4_P2TAM_DEV_PTQ_EE")]
    public string? P2TAM_DEV_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAPTSC_PTQ_EE")]
    public string? VAPTSC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GPTSTNEWE_PTQ_EE")]
    public string? GPTSTNEWE_PTQ_EE { get; set; }

    [JsonProperty("KS4_GPTSCNEWE_PTQ_EE")]
    public string? GPTSCNEWE_PTQ_EE { get; set; }

    [JsonProperty("KS4_B8SCRPLUSBONUS_PTQ_EE")]
    public string? B8SCRPLUSBONUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBPTSENG_PTQ_EE")]
    public string? EBPTSENG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBPTSMAT_PTQ_EE")]
    public string? EBPTSMAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBPTSSCI_PTQ_EE")]
    public string? EBPTSSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBPTSHUM_PTQ_EE")]
    public string? EBPTSHUM_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBPTSLAN_PTQ_EE")]
    public string? EBPTSLAN_PTQ_EE { get; set; }

    [JsonProperty("KS4_B8VAPRED_PTQ_EE")]
    public string? B8VAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_B8VASCR_PTQ_EE")]
    public string? B8VASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGVAPRED_PTQ_EE")]
    public string? ENGVAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGVASCR_PTQ_EE")]
    public string? ENGVASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_MATVAPRED_PTQ_EE")]
    public string? MATVAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_MATVASCR_PTQ_EE")]
    public string? MATVASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_SCIVAPRED_PTQ_EE")]
    public string? SCIVAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_SCIVASCR_PTQ_EE")]
    public string? SCIVASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_HUMVAPRED_PTQ_EE")]
    public string? HUMVAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_HUMVASCR_PTQ_EE")]
    public string? HUMVASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_LANVAPRED_PTQ_EE")]
    public string? LANVAPRED_PTQ_EE { get; set; }

    [JsonProperty("KS4_LANVASCR_PTQ_EE")]
    public string? LANVASCR_PTQ_EE { get; set; }

    [JsonProperty("KS4_PRIORBAND_PTQ_EE")]
    public string? PRIORBAND_PTQ_EE { get; set; }

    [JsonProperty("KS4_EALGRP_PTQ_EE")]
    public string? EALGRP_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGPAIR_PTQ_EE")]
    public string? ENGPAIR_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGPAIRLEV1_PTQ_EE")]
    public string? ENGPAIRLEV1_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGPAIRLEV2_94")]
    public string? ENGPAIRLEV2_94 { get; set; }

    [JsonProperty("KS4_ENGPAIRLEV2_95")]
    public string? ENGPAIRLEV2_95 { get; set; }

    [JsonProperty("KS4_APELIT_91")]
    public string? APELIT_91 { get; set; }

    [JsonProperty("KS4_APFOOD_91")]
    public string? APFOOD_91 { get; set; }

    [JsonProperty("KS4_APART_91")]
    public string? APART_91 { get; set; }

    [JsonProperty("KS4_APHIS_91")]
    public string? APHIS_91 { get; set; }

    [JsonProperty("KS4_APGEO_91")]
    public string? APGEO_91 { get; set; }

    [JsonProperty("KS4_APFRE_91")]
    public string? APFRE_91 { get; set; }

    [JsonProperty("KS4_APGER_91")]
    public string? APGER_91 { get; set; }

    [JsonProperty("KS4_APBUS_91")]
    public string? APBUS_91 { get; set; }

    [JsonProperty("KS4_APRS_91")]
    public string? APRS_91 { get; set; }

    [JsonProperty("KS4_APPE_91")]
    public string? APPE_91 { get; set; }

    [JsonProperty("KS4_APPHY_91")]
    public string? APPHY_91 { get; set; }

    [JsonProperty("KS4_APCHE_91")]
    public string? APCHE_91 { get; set; }

    [JsonProperty("KS4_APBIO_91")]
    public string? APBIO_91 { get; set; }

    [JsonProperty("KS4_APDRA_91")]
    public string? APDRA_91 { get; set; }

    [JsonProperty("KS4_APSPAN_91")]
    public string? APSPAN_91 { get; set; }

    [JsonProperty("KS4_APMUS_91")]
    public string? APMUS_91 { get; set; }

    [JsonProperty("KS4_APMAT_91")]
    public string? APMAT_91 { get; set; }

    [JsonProperty("KS4_APENG_91")]
    public string? APENG_91 { get; set; }

    [JsonProperty("KS4_APSTAT_91")]
    public string? APSTAT_91 { get; set; }

    [JsonProperty("KS4_APMFT_91")]
    public string? APMFT_91 { get; set; }

    [JsonProperty("KS4_APITA_91")]
    public string? APITA_91 { get; set; }

    [JsonProperty("KS4_APMGRK_91")]
    public string? APMGRK_91 { get; set; }

    [JsonProperty("KS4_APPOR_PTQ_EE")]
    public string? APPOR_PTQ_EE { get; set; }

    [JsonProperty("KS4_APARA_91")]
    public string? APARA_91 { get; set; }

    [JsonProperty("KS4_APBEN_91")]
    public string? APBEN_91 { get; set; }

    [JsonProperty("KS4_APCHI_91")]
    public string? APCHI_91 { get; set; }

    [JsonProperty("KS4_APGUJ_PTQ_EE")]
    public string? APGUJ_PTQ_EE { get; set; }

    [JsonProperty("KS4_APJAP_91")]
    public string? APJAP_91 { get; set; }

    [JsonProperty("KS4_APMHEB_91")]
    public string? APMHEB_91 { get; set; }

    [JsonProperty("KS4_APPAN_91")]
    public string? APPAN_91 { get; set; }

    [JsonProperty("KS4_APPOL_91")]
    public string? APPOL_91 { get; set; }

    [JsonProperty("KS4_APRUS_91")]
    public string? APRUS_91 { get; set; }

    [JsonProperty("KS4_APTUR_PTQ_EE")]
    public string? APTUR_PTQ_EE { get; set; }

    [JsonProperty("KS4_APURD_91")]
    public string? APURD_91 { get; set; }

    [JsonProperty("KS4_APPER_PTQ_EE")]
    public string? APPER_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCGRK_91")]
    public string? APCGRK_91 { get; set; }

    [JsonProperty("KS4_APLAT_91")]
    public string? APLAT_91 { get; set; }

    [JsonProperty("KS4_APBHEB_PTQ_EE")]
    public string? APBHEB_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCOMBSCI_91")]
    public string? APCOMBSCI_91 { get; set; }

    [JsonProperty("KS4_ENGLISHBONUS_PTQ_EE")]
    public string? ENGLISHBONUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_MATHSBONUS_PTQ_EE")]
    public string? MATHSBONUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_HPGENG_PTQ_EE")]
    public string? HPGENG_PTQ_EE { get; set; }

    [JsonProperty("KS4_HPGMATH_PTQ_EE")]
    public string? HPGMATH_PTQ_EE { get; set; }

    [JsonProperty("KS4_KS2ENG24P_PTQ_EE")]
    public string? KS2ENG24P_PTQ_EE { get; set; }

    [JsonProperty("KS4_KS2MAT24P_PTQ_EE")]
    public string? KS2MAT24P_PTQ_EE { get; set; }

    [JsonProperty("KS4_FSM")]
    public string? FSM { get; set; }

    [JsonProperty("KS4_FSM6")]
    public string? FSM6 { get; set; }

    [JsonProperty("KS4_FSM6_P")]
    public string? FSM6_P { get; set; }

    [JsonProperty("KS4_SENPS")]
    public string? SENPS { get; set; }

    [JsonProperty("KS4_SENA")]
    public string? SENA { get; set; }

    [JsonProperty("KS4_SENK")]
    public string? SENK { get; set; }

    [JsonProperty("KS4_SENE")]
    public string? SENE { get; set; }

    [JsonProperty("KS4_FEMALE")]
    public string? FEMALE { get; set; }

    [JsonProperty("KS4_FLANG")]
    public string? FLANG { get; set; }

    [JsonProperty("KS4_TRIPLESCI_E")]
    public string? TRIPLESCI_E { get; set; }

    [JsonProperty("KS4_MULTILAN_E")]
    public string? MULTILAN_E { get; set; }

    [JsonProperty("KS4_NEWMOBILE")]
    public string? NEWMOBILE { get; set; }

    [JsonProperty("KS4_KS2EMFG")]
    public string? KS2EMFG { get; set; }

    [JsonProperty("KS4_KS2EMFG_GRP")]
    public string? KS2EMFG_GRP { get; set; }

    [JsonProperty("KS4_INP8MOD")]
    public string? INP8MOD { get; set; }

    [JsonProperty("KS4_INP8CALC")]
    public string? INP8CALC { get; set; }

    [JsonProperty("KS4_ENGRESID")]
    public string? ENGRESID { get; set; }

    [JsonProperty("KS4_ENGRESID_G")]
    public string? ENGRESID_G { get; set; }

    [JsonProperty("KS4_EBAC4")]
    public string? EBAC4 { get; set; }

    [JsonProperty("KS4_EBAC5")]
    public string? EBAC5 { get; set; }

    [JsonProperty("KS4_EBAC6")]
    public string? EBAC6 { get; set; }

    [JsonProperty("KS4_EBAC4_G")]
    public string? EBAC4_G { get; set; }

    [JsonProperty("KS4_EBAC5_G")]
    public string? EBAC5_G { get; set; }

    [JsonProperty("KS4_EBAC6_G")]
    public string? EBAC6_G { get; set; }

    [JsonProperty("KS4_OTH1")]
    public string? OTH1 { get; set; }

    [JsonProperty("KS4_OTH2")]
    public string? OTH2 { get; set; }

    [JsonProperty("KS4_OTH3")]
    public string? OTH3 { get; set; }

    [JsonProperty("KS4_OTH1_G")]
    public string? OTH1_G { get; set; }

    [JsonProperty("KS4_OTH2_G")]
    public string? OTH2_G { get; set; }

    [JsonProperty("KS4_OTH3_G")]
    public string? OTH3_G { get; set; }

    [JsonProperty("KS4_SLOT1ENG")]
    public string? SLOT1ENG { get; set; }

    [JsonProperty("KS4_SLOT2MAT")]
    public string? SLOT2MAT { get; set; }

    [JsonProperty("KS4_SLOT3EBAC")]
    public string? SLOT3EBAC { get; set; }

    [JsonProperty("KS4_SLOT4EBAC")]
    public string? SLOT4EBAC { get; set; }

    [JsonProperty("KS4_SLOT5EBAC")]
    public string? SLOT5EBAC { get; set; }

    [JsonProperty("KS4_SLOT6OPEN")]
    public string? SLOT6OPEN { get; set; }

    [JsonProperty("KS4_SLOT7OPEN")]
    public string? SLOT7OPEN { get; set; }

    [JsonProperty("KS4_SLOT8OPEN")]
    public string? SLOT8OPEN { get; set; }

    [JsonProperty("KS4_SLOT6OPEN_G")]
    public string? SLOT6OPEN_G { get; set; }

    [JsonProperty("KS4_SLOT7OPEN_G")]
    public string? SLOT7OPEN_G { get; set; }

    [JsonProperty("KS4_SLOT8OPEN_G")]
    public string? SLOT8OPEN_G { get; set; }

    [JsonProperty("KS4_ATT8")]
    public string? ATT8 { get; set; }

    [JsonProperty("KS4_EBACFILL")]
    public string? EBACFILL { get; set; }

    [JsonProperty("KS4_OPENFILL")]
    public string? OPENFILL { get; set; }

    [JsonProperty("KS4_EBACSCR")]
    public string? EBACSCR { get; set; }

    [JsonProperty("KS4_OPENSCR")]
    public string? OPENSCR { get; set; }

    [JsonProperty("KS4_ATT8_PRED")]
    public string? ATT8_PRED { get; set; }

    [JsonProperty("KS4_P8SCORE")]
    public string? P8SCORE { get; set; }

    [JsonProperty("KS4_P8SCORE_ORIG")]
    public string? P8SCORE_ORIG { get; set; }

    [JsonProperty("KS4_P8ADJ")]
    public string? P8ADJ { get; set; }

    [JsonProperty("KS4_AVGP8_GRP")]
    public string? AVGP8_GRP { get; set; }

    [JsonProperty("KS4_SIGMAP8_GRP")]
    public string? SIGMAP8_GRP { get; set; }

    [JsonProperty("KS4_P8THRESH")]
    public string? P8THRESH { get; set; }

    [JsonProperty("KS4_ENG_PRED")]
    public string? ENG_PRED { get; set; }

    [JsonProperty("KS4_P8ENG")]
    public string? P8ENG { get; set; }

    [JsonProperty("KS4_MAT_PRED")]
    public string? MAT_PRED { get; set; }

    [JsonProperty("KS4_P8MAT")]
    public string? P8MAT { get; set; }

    [JsonProperty("KS4_EBAC_PRED")]
    public string? EBAC_PRED { get; set; }

    [JsonProperty("KS4_P8EBAC")]
    public string? P8EBAC { get; set; }

    [JsonProperty("KS4_OPEN_PRED")]
    public string? OPEN_PRED { get; set; }

    [JsonProperty("KS4_P8OPEN")]
    public string? P8OPEN { get; set; }

    [JsonProperty("KS4_OPENGSCR")]
    public string? OPENGSCR { get; set; }

    [JsonProperty("KS4_OPENNGSCR")]
    public string? OPENNGSCR { get; set; }

    [JsonProperty("KS4_2BTECEDEXDA_PTQ_EE")]
    public string? KS4_2BTECEDEXDA_PTQ_EE { get; set; }

    [JsonProperty("KS4_APARA_PTQ_EE")]
    public string? KS4_APARA_PTQ_EE { get; set; }

    [JsonProperty("KS4_APART_PTQ_EE")]
    public string? KS4_APART_PTQ_EE { get; set; }

    [JsonProperty("KS4_APBEN_PTQ_EE")]
    public string? KS4_APBEN_PTQ_EE { get; set; }

    [JsonProperty("KS4_APBIO_PTQ_EE")]
    public string? KS4_APBIO_PTQ_EE { get; set; }

    [JsonProperty("KS4_APBIOE_PTQ_EE")]
    public string? KS4_APBIOE_PTQ_EE { get; set; }

    [JsonProperty("KS4_APBUS_PTQ_EE")]
    public string? KS4_APBUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCHE_PTQ_EE")]
    public string? KS4_APCHE_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCHI_PTQ_EE")]
    public string? KS4_APCHI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APDRA_PTQ_EE")]
    public string? KS4_APDRA_PTQ_EE { get; set; }

    [JsonProperty("KS4_APDTT_PTQ_EE")]
    public string? KS4_APDTT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APDUT_PTQ_EE")]
    public string? KS4_APDUT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APELEC_PTQ_EE")]
    public string? KS4_APELEC_PTQ_EE { get; set; }

    [JsonProperty("KS4_APELIT_PTQ_EE")]
    public string? KS4_APELIT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APENG_PTQ_EE")]
    public string? KS4_APENG_PTQ_EE { get; set; }

    [JsonProperty("KS4_APFOOD_PTQ_EE")]
    public string? KS4_APFOOD_PTQ_EE { get; set; }

    [JsonProperty("KS4_APFRE_PTQ_EE")]
    public string? KS4_APFRE_PTQ_EE { get; set; }

    [JsonProperty("KS4_APGEO_PTQ_EE")]
    public string? KS4_APGEO_PTQ_EE { get; set; }

    [JsonProperty("KS4_APGER_PTQ_EE")]
    public string? KS4_APGER_PTQ_EE { get; set; }

    [JsonProperty("KS4_APGRA_PTQ_EE")]
    public string? KS4_APGRA_PTQ_EE { get; set; }

    [JsonProperty("KS4_APHECD_PTQ_EE")]
    public string? KS4_APHECD_PTQ_EE { get; set; }

    [JsonProperty("KS4_APHIS_PTQ_EE")]
    public string? KS4_APHIS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APIT_PTQ_EE")]
    public string? KS4_APIT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APITA_PTQ_EE")]
    public string? KS4_APITA_PTQ_EE { get; set; }

    [JsonProperty("KS4_APJAP_PTQ_EE")]
    public string? KS4_APJAP_PTQ_EE { get; set; }

    [JsonProperty("KS4_APMAT_PTQ_EE")]
    public string? KS4_APMAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APMFT_PTQ_EE")]
    public string? KS4_APMFT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APMGRK_PTQ_EE")]
    public string? KS4_APMGRK_PTQ_EE { get; set; }

    [JsonProperty("KS4_APMHEB_PTQ_EE")]
    public string? KS4_APMHEB_PTQ_EE { get; set; }

    [JsonProperty("KS4_APMUS_PTQ_EE")]
    public string? KS4_APMUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APOFT_PTQ_EE")]
    public string? KS4_APOFT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APPAN_PTQ_EE")]
    public string? KS4_APPAN_PTQ_EE { get; set; }

    [JsonProperty("KS4_APPE_PTQ_EE")]
    public string? KS4_APPE_PTQ_EE { get; set; }

    [JsonProperty("KS4_APPHY_PTQ_EE")]
    public string? KS4_APPHY_PTQ_EE { get; set; }

    [JsonProperty("KS4_APPOL_PTQ_EE")]
    public string? KS4_APPOL_PTQ_EE { get; set; }

    [JsonProperty("KS4_APRE_PTQ_EE")]
    public string? KS4_APRE_PTQ_EE { get; set; }

    [JsonProperty("KS4_APRES_PTQ_EE")]
    public string? KS4_APRES_PTQ_EE { get; set; }

    [JsonProperty("KS4_APRS_PTQ_EE")]
    public string? KS4_APRS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APSPAN_PTQ_EE")]
    public string? KS4_APSPAN_PTQ_EE { get; set; }

    [JsonProperty("KS4_APSTAT_PTQ_EE")]
    public string? KS4_APSTAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APSYS_PTQ_EE")]
    public string? KS4_APSYS_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBAC2SCI_PTQ_EE")]
    public string? KS4_EBAC2SCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBAC2SCIAG_PTQ_EE")]
    public string? KS4_EBAC2SCIAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACC_PTQ_EE")]
    public string? KS4_EBACC_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACCAG_PTQ_EE")]
    public string? KS4_EBACCAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACHUM_PTQ_EE")]
    public string? KS4_EBACHUM_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACHUMAG_PTQ_EE")]
    public string? KS4_EBACHUMAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACLAN_PTQ_EE")]
    public string? KS4_EBACLAN_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACLANAG_PTQ_EE")]
    public string? KS4_EBACLANAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACMAT_PTQ_EE")]
    public string? KS4_EBACMAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_EBACMATAG_PTQ_EE")]
    public string? KS4_EBACMATAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_EDEXDA_PTQ_EE")]
    public string? KS4_EDEXDA_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENGPAIRLEV2_PTQ_EE")]
    public string? KS4_ENGPAIRLEV2_PTQ_EE { get; set; }

    [JsonProperty("KS4_ENT1GMFL_PTQ_EE")]
    public string? KS4_ENT1GMFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_FIVEAC_PTQ_EE")]
    public string? KS4_FIVEAC_PTQ_EE { get; set; }

    [JsonProperty("KS4_FIVEAG_PTQ_EE")]
    public string? KS4_FIVEAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_A_PTQ_EE")]
    public string? KS4_GCSE_A_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_AA_PTQ_EE")]
    public string? KS4_GCSE_AA_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_AC_PTQ_EE")]
    public string? KS4_GCSE_AC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_AG_PTQ_EE")]
    public string? KS4_GCSE_AG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_ASTAR_PTQ_EE")]
    public string? KS4_GCSE_ASTAR_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_B_PTQ_EE")]
    public string? KS4_GCSE_B_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_C_PTQ_EE")]
    public string? KS4_GCSE_C_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_D_PTQ_EE")]
    public string? KS4_GCSE_D_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_DG_PTQ_EE")]
    public string? KS4_GCSE_DG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_E_PTQ_EE")]
    public string? KS4_GCSE_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_ENGAC_PTQ_EE")]
    public string? KS4_GCSE_ENGAC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_ENGAG_PTQ_EE")]
    public string? KS4_GCSE_ENGAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_F_PTQ_EE")]
    public string? KS4_GCSE_F_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_G_PTQ_EE")]
    public string? KS4_GCSE_G_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_MATHAC_PTQ_EE")]
    public string? KS4_GCSE_MATHAC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_MATHAG_PTQ_EE")]
    public string? KS4_GCSE_MATHAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_SCIAC_PTQ_EE")]
    public string? KS4_GCSE_SCIAC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSE_SCIAG_PTQ_EE")]
    public string? KS4_GCSE_SCIAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GCSEVOC_PTQ_EE")]
    public string? KS4_GCSEVOC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_A_PTQ_EE")]
    public string? KS4_GVOC_A_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_AA_PTQ_EE")]
    public string? KS4_GVOC_AA_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_AC_PTQ_EE")]
    public string? KS4_GVOC_AC_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_AG_PTQ_EE")]
    public string? KS4_GVOC_AG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_ASTAR_PTQ_EE")]
    public string? KS4_GVOC_ASTAR_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_B_PTQ_EE")]
    public string? KS4_GVOC_B_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_C_PTQ_EE")]
    public string? KS4_GVOC_C_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_D_PTQ_EE")]
    public string? KS4_GVOC_D_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_DG_PTQ_EE")]
    public string? KS4_GVOC_DG_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_E_PTQ_EE")]
    public string? KS4_GVOC_E_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_F_PTQ_EE")]
    public string? KS4_GVOC_F_PTQ_EE { get; set; }

    [JsonProperty("KS4_GVOC_G_PTQ_EE")]
    public string? KS4_GVOC_G_PTQ_EE { get; set; }

    [JsonProperty("KS4_HGMATH_PTQ_EE")]
    public string? KS4_HGMATH_PTQ_EE { get; set; }

    [JsonProperty("KS4_KS4SCI_PTQ_EE")]
    public string? KS4_KS4SCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_L2BASICS_1_PTQ_EE")]
    public string? KS4_L2BASICS_1_PTQ_EE { get; set; }

    [JsonProperty("KS4_L2BASICS_2_PTQ_EE")]
    public string? KS4_L2BASICS_2_PTQ_EE { get; set; }

    [JsonProperty("KS4_L2BASICS_3_PTQ_EE")]
    public string? KS4_L2BASICS_3_PTQ_EE { get; set; }

    [JsonProperty("KS4_L2BASICS_4_PTQ_EE")]
    public string? KS4_L2BASICS_4_PTQ_EE { get; set; }

    [JsonProperty("KS4_L2BASICS_LL_PTQ_EE")]
    public string? KS4_L2BASICS_LL_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2EM_PTQ_EE")]
    public string? KS4_LEV2EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCI2_PTQ_EE")]
    public string? KS4_LEV2SCI2_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCI2B_PTQ_EE")]
    public string? KS4_LEV2SCI2B_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIA_PTQ_EE")]
    public string? KS4_LEV2SCIA_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIB_PTQ_EE")]
    public string? KS4_LEV2SCIB_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIC_PTQ_EE")]
    public string? KS4_LEV2SCIC_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCID_PTQ_EE")]
    public string? KS4_LEV2SCID_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIE_PTQ_EE")]
    public string? KS4_LEV2SCIE_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIF_PTQ_EE")]
    public string? KS4_LEV2SCIF_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEV2SCIG_PTQ_EE")]
    public string? KS4_LEV2SCIG_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEVEL2_EM_PTQ_EE")]
    public string? KS4_LEVEL2_EM_PTQ_EE { get; set; }

    [JsonProperty("KS4_LEVEL2MFL_PTQ_EE")]
    public string? KS4_LEVEL2MFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_MATHPAIR_PTQ_EE")]
    public string? KS4_MATHPAIR_PTQ_EE { get; set; }

    [JsonProperty("KS4_MFL_PTQ_EE")]
    public string? KS4_MFL_PTQ_EE { get; set; }

    [JsonProperty("KS4_ONEAG_PTQ_EE")]
    public string? KS4_ONEAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_1AC_PTQ_EE")]
    public string? KS4_PASS_1AC_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_AASTAR_PTQ_EE")]
    public string? KS4_PASS_AASTAR_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_AASTAR5_PTQ_EE")]
    public string? KS4_PASS_AASTAR5_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCI2_PTQ_EE")]
    public string? KS4_PASS_ABSCI2_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCI2B_PTQ_EE")]
    public string? KS4_PASS_ABSCI2B_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCIA_PTQ_EE")]
    public string? KS4_PASS_ABSCIA_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCIB_PTQ_EE")]
    public string? KS4_PASS_ABSCIB_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCIC_PTQ_EE")]
    public string? KS4_PASS_ABSCIC_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCID_PTQ_EE")]
    public string? KS4_PASS_ABSCID_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCIE_PTQ_EE")]
    public string? KS4_PASS_ABSCIE_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_ABSCIF_PTQ_EE")]
    public string? KS4_PASS_ABSCIF_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_AC_3NG_PTQ_EE")]
    public string? KS4_PASS_AC_3NG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS_AG_PTQ_EE")]
    public string? KS4_PASS_AG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS2SCIA_PTQ_EE")]
    public string? KS4_PASS2SCIA_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS2SCIAAG_PTQ_EE")]
    public string? KS4_PASS2SCIAAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS2SCIB_PTQ_EE")]
    public string? KS4_PASS2SCIB_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASS2SCIBAG_PTQ_EE")]
    public string? KS4_PASS2SCIBAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASSMATHPR_PTQ_EE")]
    public string? KS4_PASSMATHPR_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASSMATHPRAG_PTQ_EE")]
    public string? KS4_PASSMATHPRAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASSOTHSCI_PTQ_EE")]
    public string? KS4_PASSOTHSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_PASSOTHSCIAG_PTQ_EE")]
    public string? KS4_PASSOTHSCIAG_PTQ_EE { get; set; }

    [JsonProperty("KS4_VAP2TASCI_PTQ_EE")]
    public string? KS4_VAP2TASCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APBHEB_91")]
    public string? APBHEB_91 { get; set; }

    [JsonProperty("KS4_APGUJ_91")]
    public string? APGUJ_91 { get; set; }

    [JsonProperty("KS4_APPER_91")]
    public string? APPER_91 { get; set; }

    [JsonProperty("KS4_APPOR_91")]
    public string? APPOR_91 { get; set; }

    [JsonProperty("KS4_APTUR_91")]
    public string? APTUR_91 { get; set; }

    [JsonProperty("KS4_EBACCSUBJENT_0")]
    public string? EBACCSUBJENT_0 { get; set; }

    [JsonProperty("KS4_EBACCSUBJENT_1")]
    public string? EBACCSUBJENT_1 { get; set; }

    [JsonProperty("KS4_EBACCSUBJENT_2")]
    public string? EBACCSUBJENT_2 { get; set; }

    [JsonProperty("KS4_EBACCSUBJENT_3")]
    public string? EBACCSUBJENT_3 { get; set; }

    [JsonProperty("KS4_EBACCSUBJENT_4")]
    public string? EBACCSUBJENT_4 { get; set; }

    [JsonProperty("KS4_ENTBASICS")]
    public string? ENTBASICS { get; set; }

    [JsonProperty("KS4_ENTERED_ART")]
    public string? ENTERED_ART { get; set; }

    [JsonProperty("KS4_ENTERED_GEOGRAPHY")]
    public string? ENTERED_GEOGRAPHY { get; set; }

    [JsonProperty("KS4_ENTERED_HIST_GEOG")]
    public string? ENTERED_HIST_GEOG { get; set; }

    [JsonProperty("KS4_ENTERED_HISTORY")]
    public string? ENTERED_HISTORY { get; set; }

    [JsonProperty("KS4_FIVE91")]
    public string? FIVE91 { get; set; }

    [JsonProperty("KS4_FIVE94")]
    public string? FIVE94 { get; set; }

    [JsonProperty("KS4_GCSE_1")]
    public string? GCSE_1 { get; set; }

    [JsonProperty("KS4_GCSE_2")]
    public string? GCSE_2 { get; set; }

    [JsonProperty("KS4_GCSE_3")]
    public string? GCSE_3 { get; set; }

    [JsonProperty("KS4_GCSE_4")]
    public string? GCSE_4 { get; set; }

    [JsonProperty("KS4_GCSE_5")]
    public string? GCSE_5 { get; set; }

    [JsonProperty("KS4_GCSE_6")]
    public string? GCSE_6 { get; set; }

    [JsonProperty("KS4_GCSE_7")]
    public string? GCSE_7 { get; set; }

    [JsonProperty("KS4_GCSE_8")]
    public string? GCSE_8 { get; set; }

    [JsonProperty("KS4_GCSE_9")]
    public string? GCSE_9 { get; set; }

    [JsonProperty("KS4_GCSE_91")]
    public string? GCSE_91 { get; set; }

    [JsonProperty("KS4_GCSE_94")]
    public string? GCSE_94 { get; set; }

    [JsonProperty("KS4_GCSE_95")]
    public string? GCSE_95 { get; set; }

    [JsonProperty("KS4_KS2EMSS")]
    public string? KS2EMSS { get; set; }

    [JsonProperty("KS4_KS2EMSS_GRP")]
    public string? KS2EMSS_GRP { get; set; }

    [JsonProperty("KS4_KS2MATSCORE")]
    public string? KS2MATSCORE { get; set; }

    [JsonProperty("KS4_KS2MATSCORETA")]
    public string? KS2MATSCORETA { get; set; }

    [JsonProperty("KS4_KS2READSCORE")]
    public string? KS2READSCORE { get; set; }

    [JsonProperty("KS4_KS2READSCORETA")]
    public string? KS2READSCORETA { get; set; }

    [JsonProperty("KS4_PASS_1_91")]
    public string? PASS_1_91 { get; set; }

    [JsonProperty("KS4_PASS_91")]
    public string? PASS_91 { get; set; }

    [JsonProperty("KS4_APRUS_PTQ_EE")]
    public string? APRUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APURD_PTQ_EE")]
    public string? APURD_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCGRK_PTQ_EE")]
    public string? APCGRK_PTQ_EE { get; set; }

    [JsonProperty("KS4_APLAT_PTQ_EE")]
    public string? APLAT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APDSCI_PTQ_EE")]
    public string? APDSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APVBUS_PTQ_EE")]
    public string? APVBUS_PTQ_EE { get; set; }

    [JsonProperty("KS4_APHSC_PTQ_EE")]
    public string? APHSC_PTQ_EE { get; set; }

    [JsonProperty("KS4_APLT_PTQ_EE")]
    public string? APLT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APVSCI_PTQ_EE")]
    public string? APVSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APSVSCI_PTQ_EE")]
    public string? APSVSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APVIT_PTQ_EE")]
    public string? APVIT_PTQ_EE { get; set; }

    [JsonProperty("KS4_APCORESCI_PTQ_EE")]
    public string? APCORESCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APADTSCI_PTQ_EE")]
    public string? APADTSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_APAPDSCI_PTQ_EE")]
    public string? APAPDSCI_PTQ_EE { get; set; }

    [JsonProperty("KS4_FLAG24ENGPRG_PTQ_EE")]
    public string? FLAG24ENGPRG_PTQ_EE { get; set; }

    [JsonProperty("KS4_FLAG24MATPRG_PTQ_EE")]
    public string? FLAG24MATPRG_PTQ_EE { get; set; }

    [JsonProperty("KS4_VERSION")]
    public string? VERSION { get; set; }
}
