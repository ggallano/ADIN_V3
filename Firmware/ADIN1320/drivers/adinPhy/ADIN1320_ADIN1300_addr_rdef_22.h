/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2024 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors. By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

#ifndef ADIN_PHY_ADDR_RDEF_22_H
#define ADIN_PHY_ADDR_RDEF_22_H

/** @addtogroup adin1320 ADIN1320 PHY Driver
 *  @{
 */

/* ====================================================================================================
        ADINPHY Module Register Address Offset Definitions
   ==================================================================================================== */
#define ADDR_MII_CONTROL                             (0x0000)       /* MII Control Register*/
#define ADDR_MII_STATUS                              (0x0001)       /* MII Status Register */
#define ADDR_PHY_ID_1                                (0x0002)       /* PHY Identifier 1 Register */
#define ADDR_PHY_ID_2                                (0x0003)       /* PHY Identifier 2 Register */
#define ADDR_AUTONEG_ADV                             (0x0004)       /* Autonegotiation Advertisement Register */
#define ADDR_LP_ABILITY                              (0x0005)       /* Autonegotiation Link Partner Base Page Ability Register */
#define ADDR_AUTONEG_EXP                             (0x0006)       /* Autonegotiation Expansion Register */
#define ADDR_TX_NEXT_PAGE                            (0x0007)       /* Autonegotiation Next Page Transmit Register */
#define ADDR_LP_RX_NEXT_PAGE                         (0x0008)       /* Autonegotiation Link Partner Received Next Page Register. */ 
#define ADDR_MSTR_SLV_STATUS                         (0x000A)       /* Master Slave Status Register */
#define ADDR_EXT_STATUS                              (0x000F)       /* Extended Status Register */
#define ADDR_EXT_REG_PTR                             (0x0010)       /* Extended Register Pointer Register */
#define ADDR_EXT_REG_DATA                            (0x0011)       /* Extended Register Data Register */
#define ADDR_PHY_CTRL_1                              (0x0012)       /* PHY Control 1 Register */
#define ADDR_PHY_CTRL_STATUS_1                       (0x0013)       /* PHY Control Status 1 Register */
#define ADDR_PHY_RX_ERR_CNT                          (0x0014)       /* Receive Error Count Register */
#define ADDR_PHY_CTRL_STATUS_2                       (0x0015)       /* PHY Control Status 2 Register */
#define ADDR_PHY_CTRL_2                              (0x0016)       /* PHY Control 2 Register */
#define ADDR_PHY_CTRL_3                              (0x0017)       /* PHY Control 2 Register */
#define ADDR_IRQ_MASK                                (0x0018)       /* PHY Control 2 Register. */
#define ADDR_IRQ_STATUS                              (0x0019)       /* PHY Control 2 Register. */
#define ADDR_PHY_STATUS_1                            (0x001A)       /* PHY Control 2 Register. */
#define ADDR_LED_CTRL_1                              (0x001B)       /* LED Control 1 Register */
#define ADDR_LED_CTRL_2                              (0x001C)       /* LED Control 2 Register */
#define ADDR_LED_CTRL_3                              (0x001D)       /* LED Control 3 Register */
#define ADDR_PHY_STATUS_2                            (0x001F)       /* PHY Status 2 Register */

/* ====================================================================================================
        ADINPHY Module Register ResetValue Definitions
   ==================================================================================================== */
#define RSTVAL_MII_CONTROL                           (0x1000)              
#define RSTVAL_MII_STATUS                            (0x7949) 
#define RSTVAL_PHY_ID_1                              (0x0283) 
#define RSTVAL_PHY_ID_2                              (0xBC20) 
#define RSTVAL_AUTONEG_ADV                           (0x01E1) 
#define RSTVAL_LP_ABILITY                            (0x0000) 
#define RSTVAL_AUTONEG_EXP                           (0x0064) 
#define RSTVAL_TX_NEXT_PAGE                          (0x2001) 
#define RSTVAL_LP_RX_NEXT_PAGE                       (0x0000) 
#define RSTVAL_MSTR_SLV_STATUS                       (0x0000) 
#define RSTVAL_EXT_STATUS                            (0x0000) 
#define RSTVAL_EXT_REG_PTR                           (0x0000) 
#define RSTVAL_EXT_REG_DATA                          (0x0000) 
#define RSTVAL_PHY_CTRL_1                            (0x0002) 
#define RSTVAL_PHY_CTRL_STATUS_1                     (0x1041) 
//#define RSTVAL_RX_ERR_CNT                            (0x0000)
#define RSTVAL_PHY_CTRL_STATUS_2                     (0x0000) 
#define RSTVAL_PHY_CTRL_2                            (0x0308) 
#define RSTVAL_PHY_CTRL_3                            (0x3048) 
#define RSTVAL_IRQ_MASK Interrupt                    (0x0000) 
#define RSTVAL_IRQ_STATUS                            (0x0000) 
#define RSTVAL_PHY_STATUS_1                          (0x0300) 
#define RSTVAL_LED_CTRL_1                            (0x0001) 
#define RSTVAL_LED_CTRL_2                            (0x210A) 
#define RSTVAL_LED_CTRL_3                            (0x1855) 
#define RSTVAL_PHY_STATUS_2                          (0x03FC)

/* ====================================================================================================
        ADINPHY Module Register BitPositions, Lengths, Masks and Enumerations Definitions
   ==================================================================================================== */

/* ----------------------------------------------------------------------------------------------------
          MII_CONTROL                                Value             Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_MII_CONTROL_SFT_RST                     (15U)          /* Software Reset Bit */
#define BITL_MII_CONTROL_SFT_RST                     (1U)           
#define BITM_MII_CONTROL_SFT_RST                     (0X8000U)      

#define BITP_MII_CONTROL_LOOPBACK                    (14U)          /* LoopBack */
#define BITL_MII_CONTROL_LOOPBACK                    (1U)           
#define BITM_MII_CONTROL_LOOPBACK                    (0X4000U)      

#define BITP_MII_CONTROL_SPEED_SEL_LSB               (13U)          /* The speed selection LSB register bits */
#define BITL_MII_CONTROL_SPEED_SEL_LSB               (1U)           
#define BITM_MII_CONTROL_SPEED_SEL_LSB               (0X2000U)      
     
#define BITP_MII_CONTROL_AUTONEG_EN                  (12U)          /* Autonegotiation enable bit */
#define BITL_MII_CONTROL_AUTONEG_EN                  (1U)           
#define BITM_MII_CONTROL_AUTONEG_EN                  (0X1000U)      

#define BITP_MII_CONTROL_SFT_PD                      (11U)          /* Software Power-down */
#define BITL_MII_CONTROL_SFT_PD                      (1U)           
#define BITM_MII_CONTROL_SFT_PD                      (0X0800U)      

#define BITP_MII_CONTROL_ISOLATE                     (10U)          /* Software Power-down */
#define BITL_MII_CONTROL_ISOLATE                     (1U)           
#define BITM_MII_CONTROL_ISOLATE                     (0X0400U)      

#define BITP_MII_CONTROL_RESTART_ANEG                (9U)           /* Restart Autonegotiation Bit */
#define BITL_MII_CONTROL_RESTART_ANEG                (1U)           
#define BITM_MII_CONTROL_RESTART_ANEG                (0X0200U)      
     
#define BITP_MII_CONTROL_DPLX_MODE                   (8U)           /* Duplex Mode Bit. */
#define BITL_MII_CONTROL_DPLX_MODE                   (1U)           
#define BITM_MII_CONTROL_DPLX_MODE                   (0X0100U) 
          
#define BITP_MII_CONTROL_COLTEST                     (7U)           /* Col test Bit. */
#define BITL_MII_CONTROL_COLTEST                     (1U)           
#define BITM_MII_CONTROL_COLTEST                     (0X0080U)

#define BITP_MII_CONTROL_SPEED_SEL_MSB               (6U)           /* The speed selection MSB */
#define BITL_MII_CONTROL_SPEED_SEL_MSB               (1U)           
#define BITM_MII_CONTROL_SPEED_SEL_MSB               (0X0040U)           
     
#define BITP_MII_CONTROL_UNIDIR_EN                   (5U)           /* Uni Dir Bit */
#define BITL_MII_CONTROL_UNIDIR_EN                   (1U)           
#define BITM_MII_CONTROL_UNIDIR_EN                   (0X0020U)     

/* ----------------------------------------------------------------------------------------------------
          MII_STATUS                                  Value             Description
   ---------------------------------------------------------------------------------------------------- */     
#define BITP_MII_STATUS_T_4_SPRT                     (15U)          /* T4 SPRT Bit */
#define BITL_MII_STATUS_T_4_SPRT                     (1U)           /* T4 SPRT Bit */
#define BITM_MII_STATUS_T_4_SPRT                     (0X8000U)      /* T4 SPRT Bit */

#define BITP_MII_STATUS_FD_100_SPRT                  (14U)          /* FD_100_SPRT */
#define BITL_MII_STATUS_FD_100_SPRT                  (1U)           
#define BITM_MII_STATUS_FD_100_SPRT                  (0X4000U)      

#define BITP_MII_STATUS_HD_100_SPRT                  (13U)          /* HD_100_SPRT  Bit */
#define BITL_MII_STATUS_HD_100_SPRT                  (1U)           
#define BITM_MII_STATUS_HD_100_SPRT                  (0X2000U)      

#define BITP_MII_STATUS_FD_10_SPRT                   (12U)          /* FD_10_SPRT Bit */
#define BITL_MII_STATUS_FD_10_SPRT                   (1U)           
#define BITM_MII_STATUS_FD_10_SPRT                   (0X1000U)      

#define BITP_MII_STATUS_HD_10_SPRT                   (11U)          /* HD_10_SPRT Bit */
#define BITL_MII_STATUS_HD_10_SPRT                   (1U)           
#define BITM_MII_STATUS_HD_10_SPRT                   (0X0800U)       

#define BITP_MII_STATUS_FD_T_2_SPRT                  (10U)          /* FD_T_2_SPRT Bit */
#define BITL_MII_STATUS_FD_T_2_SPRT                  (1U)           
#define BITM_MII_STATUS_FD_T_2_SPRT                  (0X0400U)      

#define BITP_MII_STATUS_HD_T_2_SPRT                  (9U)           /* HD_T_2_SPRT Bit */
#define BITL_MII_STATUS_HD_T_2_SPRT                  (1U)           
#define BITM_MII_STATUS_HD_T_2_SPRT                  (0X0200U)      

#define BITP_MII_STATUS_EXT_STAT_SPRT                (8U)           /* EXT_STAT_SPRT Bit */
#define BITL_MII_STATUS_EXT_STAT_SPRT                (1U)           
#define BITM_MII_STATUS_EXT_STAT_SPRT                (0X0100U)      

#define BITP_MII_STATUS_UNIDIR_ABLE                  (7U)           /* UNIDIR_ABLE Bit */
#define BITL_MII_STATUS_UNIDIR_ABLE                  (1U)           
#define BITM_MII_STATUS_UNIDIR_ABLE                  (0X0080U)      

#define BITP_MII_STATUS_MF_PREAM_SUP_ABLE            (6U)           /* MF_PREAM_SUP_ABLE Bit */
#define BITL_MII_STATUS_MF_PREAM_SUP_ABLE            (1U)           
#define BITM_MII_STATUS_MF_PREAM_SUP_ABLE            (0X0040U)

#define BITP_MII_STATUS_AUTONEG_DONE                 (5U)           /* Autonegotiation Done Bit */
#define BITL_MII_STATUS_AUTONEG_DONE                 (1U)           
#define BITM_MII_STATUS_AUTONEG_DONE                 (0X0020U)      
     
#define BITP_MII_STATUS_REM_FLT_LAT                  (4U)           /* REM_FLT_LAT Bit */
#define BITL_MII_STATUS_REM_FLT_LAT                  (1U)           
#define BITM_MII_STATUS_REM_FLT_LAT                  (0X0010U)      

#define BITP_MII_STATUS_AUTONEG_ABLE                 (3U)           /* AUTONEG_ABLE Bit */
#define BITL_MII_STATUS_AUTONEG_ABLE                 (1U)           
#define BITM_MII_STATUS_AUTONEG_ABLE                 (0X0008U)      

#define BITP_MII_STATUS_LINK_STAT_LAT                (2U)           /* LINK_STAT_LAT Bit */
#define BITL_MII_STATUS_LINK_STAT_LAT                (1U)           
#define BITM_MII_STATUS_LINK_STAT_LAT                (0X0004U)      

#define BITP_MII_STATUS_JABBER_DET_LAT               (1U)           /* JABBER_DET_LAT Bit. */
#define BITL_MII_STATUS_JABBER_DET_LAT               (1U)           
#define BITM_MII_STATUS_JABBER_DET_LAT               (0X0002U)      

#define BITP_MII_STATUS_EXT_CAPABLE                  (0U)           /* EXT_CAPABLE Bit */
#define BITL_MII_STATUS_EXT_CAPABLE                  (1U)           
#define BITM_MII_STATUS_EXT_CAPABLE                  (0X0001U)      

/* ----------------------------------------------------------------------------------------------------
          PHY_ID_1                                   Value             Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_PHY_ID_1_PHY_ID_1                       (0U)           /* Organizationally Unique Identifier */
#define BITL_PHY_ID_1_PHY_ID_1                       (16U)
#define BITM_PHY_ID_1_PHY_ID_1                       (0XFFFFU)

/* ----------------------------------------------------------------------------------------------------
          PHY_ID_2                                   Value             Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_PHY_ID_2_PHY_ID_2_OUI                   (10U)          /* Organizationally Unique Identifier */
#define BITL_PHY_ID_2_PHY_ID_2_OUI                   (6U)
#define BITM_PHY_ID_2_PHY_ID_2_OUI                   (0XFC00U)

#define BITP_PHY_ID_2_MODEL_NUM                      (4U)           /* Model Number */
#define BITL_PHY_ID_2_MODEL_NUM                      (6U)
#define BITM_PHY_ID_2_MODEL_NUM                      (0X03F0U)

#define BITP_PHY_ID_2_REV_NUM                        (0U)           /* Revision Number */
#define BITL_PHY_ID_2_REV_NUM                        (4U)
#define BITM_PHY_ID_2_REV_NUM                        (0X000FU)

/* ----------------------------------------------------------------------------------------------------
          EXT_REG_PTR                                Value             Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_EXT_REG_PTR_EXT_REG_PTR                 (0U)           /* EXT_REG_PTR*/
#define BITL_EXT_REG_PTR_EXT_REG_PTR                 (15U)          
#define BITM_EXT_REG_PTR_EXT_REG_PTR                 (0XFFFFU)  

/* ----------------------------------------------------------------------------------------------------
          EXT_REG_DATA                               Value             Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_EXT_REG_DATA_EXT_REG_DATA               (0U)           /* EXT_REG_DATA*/
#define BITL_EXT_REG_DATA_EXT_REG_DATA               (15U)          
#define BITM_EXT_REG_DATA_EXT_REG_DATA               (0XFFFFU)  

/* ----------------------------------------------------------------------------------------------------
          LED Control 1 Register                     Value             Description
  ---------------------------------------------------------------------------------------------------- */
#define BITP_LED_CTRL_1_LED_A_EXT_CFG_EN             (10U)          /* LED_A_EXT_CFG_EN */
#define BITL_LED_CTRL_1_LED_A_EXT_CFG_EN             (1U)          
#define BITM_LED_CTRL_1_LED_A_EXT_CFG_EN             (0X0400U) 

#define BITP_LED_CTRL_1_LED_PAT_PAUSE_DUR            (4U)           /* LED_PAT_PAUSE_DUR */
#define BITL_LED_CTRL_1_LED_PAT_PAUSE_DUR            (4U)          
#define BITM_LED_CTRL_1_LED_PAT_PAUSE_DUR            (0X00F0U) 

#define BITP_LED_CTRL_1_LED_PUL_STR_DUR_SEL          (2U)           /* LED_PUL_STR_DUR_SEL */
#define BITL_LED_CTRL_1_LED_PUL_STR_DUR_SEL          (2U)          
#define BITM_LED_CTRL_1_LED_PUL_STR_DUR_SEL          (0X000CU) 

#define BITP_LED_CTRL_1_LED_OE_N                     (1U)           /* LED_OE_N */
#define BITL_LED_CTRL_1_LED_OE_N                     (1U)          
#define BITM_LED_CTRL_1_LED_OE_N                     (0X0002U)     
     
#define BITP_LED_CTRL_1_LED_PUL_STR_EN               (0U)           /* LED_PUL_STR_ENs */
#define BITL_LED_CTRL_1_LED_PUL_STR_EN               (1U)          
#define BITM_LED_CTRL_1_LED_PUL_STR_EN               (0X0001U)     

/*----------------------------------------------------------------------------------------------------
        LED Control 2 Register                       Value           Description
---------------------------------------------------------------------------------------------------- */     
#define BITP_LED_CTRL_2_LED_A_CFG                    (0U)           /* LED_A_CFG s*/
#define BITL_LED_CTRL_2_LED_A_CFG                    (4U)          
#define BITM_LED_CTRL_2_LED_A_CFG                    (0X000FU)  


/* ====================================================================================================
        ADINPHY Clause 45 Access Registers
   ==================================================================================================== */

#define ADDR_GE_SFT_RST                              (0XFF0C)  /* Subsystem Software Reset Register */
#define ADDR_GE_SFT_RST_CFG_EN                       (0XFF0D)  /* Subsystem Software Reset Configuration Enable Register */

/* ----------------------------------------------------------------------------------------------------
          GE_SFT_RST                                  Value                 Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GE_SFT_RST_GE_SFT_RST                    (0U)                /* GE_SFT_RST */
#define BITL_GE_SFT_RST_GE_SFT_RST                    (1U)
#define BITM_GE_SFT_RST_GE_SFT_RST                    (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          GE_SFT_RST_CFG_EN                            Value              Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN      (0U)                /* GE_SFT_RST_CFG_EN */
#define BITL_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN      (1U)
#define BITM_GE_SFT_RST_CFG_EN_GE_SFT_RST_CFG_EN      (0X0001U)

/**@}*/

#endif /* ADIN_PHY_ADDR_RDEF_22_H */


