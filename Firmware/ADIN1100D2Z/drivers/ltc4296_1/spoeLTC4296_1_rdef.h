/*
 *---------------------------------------------------------------------------
 *
 * Copyright (c) 2023 Analog Devices, Inc. All Rights Reserved.
 * This software is proprietary to Analog Devices, Inc.
 * and its licensors.By using this software you agree to the terms of the
 * associated Analog Devices Software License Agreement.
 *
 *---------------------------------------------------------------------------
 */

#ifndef LTC4296_1_REG_DEFINES_H_
#define LTC4296_1_REG_DEFINES_H_

/** @addtogroup SPOE LTC4296_1 Driver
 *  @{
 */

#include <stdint.h>


/* ====================================================================================================
        LTC4296-1 Register Address Offset Definitions
   ==================================================================================================== */
#define ADDR_GFLTEV        (0x02)      /* Global Fault Event Register, Read/Write 1 to Clear. Indicates presence of global level faults.     */
#define ADDR_GFLTMSK       (0x03)      /* Global Fault Event Mask Register, Read/Write. Provides mask for the global level fault events.     */
#define ADDR_GCAP          (0x06)      /* Global Capability Register, Read Only. Presents supported features of PoDL standard.               */
#define ADDR_GIOST         (0x07)      /* Global Status Register, Read Only. Presents status of the inputs and outputs.                      */
#define ADDR_GCMD          (0x08)      /* Global Command Register, Read/Write. Entry point for the host to configure the chip.               */
#define ADDR_GCFG          (0x09)      /* Global Configuration Register, Read/Write. Enables the host to configure global functions.         */
#define ADDR_GADCCFG       (0x0A)      /* Global ADC Configuration Register, Read/Write. Allows the host to configure the global ADC.        */
#define ADDR_GADCDAT       (0x0B)      /* Global ADC Data Register, Read Only. Allows the host to read the latest global ADC measurement.    */
#define ADDR_P0EV          (0x10)      /* Port 0 Event Register, Read/Write 1 to Clear. Indicates presence of Port 0 events.                 */
#define ADDR_P0ST          (0x12)      /* Port 0 Status Register, Read Only. Provides status of Port 0 events.                               */
#define ADDR_P0CFG0        (0x13)      /* Port 0 Configuration Register 0, Read/Write. Provides Port 0 configuration.                        */
#define ADDR_P0CFG1        (0x14)      /* Port 0 Configuration Register 1, Read/Write. Provides Port 0 configuration.                        */
#define ADDR_P0ADCCFG      (0x15)      /* Port 0 ADC Configuration Register, Read/Write. Provides Port 0 ADC configuration.                  */
#define ADDR_P0ADCDAT      (0x16)      /* Port 0 ADC Data Register, Read Only. Allows the host to read the latest Port 0 ADC measurement.    */
#define ADDR_P0SELFTEST    (0x17)      /* Port 0 Self Test Register, Read/Write. Enables the host to perform diagnosis on Port 0.            */
#define ADDR_P1EV          (0x20)      /* Port 1 Event Register, Read/Write 1 to Clear. Indicates presence of Port 1 events.                 */
#define ADDR_P1ST          (0x22)      /* Port 1 Status Register, Read Only. Provides status of Port 1 events.                               */
#define ADDR_P1CFG0        (0x23)      /* Port 1 Configuration Register 0, Read/Write. Provides Port 1 configuration.                        */
#define ADDR_P1CFG1        (0x24)      /* Port 1 Configuration Register 1, Read/Write. Provides Port 1 configuration.                        */
#define ADDR_P1ADCCFG      (0x25)      /* Port 1 ADC Configuration Register, Read/Write. Provides Port 1 ADC configuration.                  */
#define ADDR_P1ADCDAT      (0x26)      /* Port 1 ADC Data Register, Read Only. Allows the host to read the latest Port 1 ADC measurement.    */
#define ADDR_P1SELFTEST    (0x27)      /* Port 1 Self Test Register, Read/Write. Enables the host to perform diagnosis on Port 1.            */
#define ADDR_P2EV          (0x30)      /* Port 2 Event Register, Read/Write 1 to Clear. Indicates presence of Port 2 events.                 */
#define ADDR_P2ST          (0x32)      /* Port 2 Status Register, Read Only. Provides status of Port 2 events.                               */
#define ADDR_P2CFG0        (0x33)      /* Port 2 Configuration Register 0, Read/Write, Provides Port 2 configuration.                        */
#define ADDR_P2CFG1        (0x34)      /* Port 2 Configuration Register 1, Read/Write. Provides Port 2 configuration.                        */
#define ADDR_P2ADCCFG      (0x35)      /* Port 2 ADC Configuration Register, Read/Write. Provides Port 2 ADC configuration.                  */
#define ADDR_P2ADCDAT      (0x36)      /* Port 2 ADC Data Register, Read Only. Allows the host to Read the Latest Port 2 ADC measurement.    */
#define ADDR_P2SELFTEST    (0x37)      /* Port 2 Self Test Register, Read/Write. Enable the host to perform diagnosis on Port 2.             */
#define ADDR_P3EV          (0x40)      /* Port 3 Event Register, Read/Write 1 to Clear. Indicates presence of Port 3 events.                 */
#define ADDR_P3ST          (0x42)      /* Port 3 Status Register, Read Only. Provides status of Port 3 events.                               */
#define ADDR_P3CFG0        (0x43)      /* Port 3 Configuration Register 0, Read/Write. Provides Port 3 Configuration.                        */
#define ADDR_P3CFG1        (0x44)      /* Port 3 Configuration Register 1, Read/Write. Provides Port 3 Configuration.                        */
#define ADDR_P3ADCCFG      (0x45)      /* Port 3 ADC Configuration Register, Read/Write. Provides Port 3 ADC configuration.                  */
#define ADDR_P3ADCDAT      (0x46)      /* Port 3 ADC Data Register, Read Only. Allows the host to read the latest Port 3 ADC measurement.    */
#define ADDR_P3SELFTEST    (0x47)      /* Port 3 Self Test Register, Read/Write. Enables the host to perform diagnosis on Port 3.            */
#define ADDR_P4EV          (0x50)      /* Port 4 Event Register, Read/Write 1 to Clear. Indicates presence of Port 4 Events.                 */
#define ADDR_P4ST          (0x52)      /* Port 4 Status Register, Read Only. Provides status of Port 4 events.                               */
#define ADDR_P4CFG0        (0x53)      /* Port 4 Configuration Register 0, Read/Write. Provides Port 4 configuration.                        */
#define ADDR_P4CFG1        (0x54)      /* Port 4 Configuration Register 1, Read/Write. Provides Port 4 Configuration.                        */
#define ADDR_P4ADCCFG      (0x55)      /* Port 4 ADC Configuration Register, Read/Write. Provides Port 4 ADC configuration.                  */
#define ADDR_P4ADCDAT      (0x56)      /* Port 4 ADC Data Register, Read Only. Allows the host to read the latest Port 4 ADC measurement.    */
#define ADDR_P4SELFTEST    (0x57)      /* Port 4 Self Test Register, Read/Write. Enables the host to perform diagnosis on Port 4.            */



/* ====================================================================================================
         LTC4296-1 Register ResetValue Definitions
   ==================================================================================================== */
#define RSTVAL_GFLTEV       (0x0010)
#define RSTVAL_GFLTMSK      (0x000F)
#define RSTVAL_GCAP         (0x0025)
#define RSTVAL_GIOST        (0x0000)
#define RSTVAL_GCMD         (0x00A0)
#define RSTVAL_GCFG         (0x0009)
#define RSTVAL_GADCCFG      (0x0000)
#define RSTVAL_GADCDAT      (0x0000)
#define RSTVAL_P0EV         (0x0000)
#define RSTVAL_P0ST         (0x0000)
#define RSTVAL_P0CFG0       (0x0002)
#define RSTVAL_P0CFG1       (0x0008)
#define RSTVAL_P0ADCCFG     (0x0006)
#define RSTVAL_P0ADCDAT     (0x0000)
#define RSTVAL_P0SELFTEST   (0x0000)
#define RSTVAL_P1EV         (0x0000)
#define RSTVAL_P1ST         (0x0000)
#define RSTVAL_P1CFG0       (0x0002)
#define RSTVAL_P1CFG1       (0x0008)
#define RSTVAL_P1ADCCFG     (0x0006)
#define RSTVAL_P1ADCDAT     (0x0000)
#define RSTVAL_P1SELFTEST   (0x0000)
#define RSTVAL_P2EV         (0x0000)
#define RSTVAL_P2ST         (0x0000)
#define RSTVAL_P2CFG0       (0x0002)
#define RSTVAL_P2CFG1       (0x0000)
#define RSTVAL_P2ADCCFG     (0x0006)
#define RSTVAL_P2ADCDAT     (0x0000)
#define RSTVAL_P2SELFTEST   (0x0000)
#define RSTVAL_P3EV         (0x0000)
#define RSTVAL_P3ST         (0x0000)
#define RSTVAL_P3CFG0       (0x0002)
#define RSTVAL_P3CFG1       (0x0008)
#define RSTVAL_P3ADCCFG     (0x0006)
#define RSTVAL_P3ADCDAT     (0x0000)
#define RSTVAL_P3SELFTEST   (0x0000)
#define RSTVAL_P4EV         (0x0000)
#define RSTVAL_P4ST         (0x0000)
#define RSTVAL_P4CFG0       (0x0002)
#define RSTVAL_P4CFG1       (0x0008)
#define RSTVAL_P4ADCCFG     (0x0006)
#define RSTVAL_P4ADCDAT     (0x0000)
#define RSTVAL_P4SELFTEST   (0x0000)




/* ====================================================================================================
         LTC4296-1 Register BitPositions, Lengths, Masks and Enumerations Definitions
   ==================================================================================================== */

/* ----------------------------------------------------------------------------------------------------
          GFLTEV   			                 Value			Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GFLTEV_UVLO_DIGITAL             (4U)          	/* Set this bit if the digital core was in undervoltage lockout. */
#define BITL_GFLTEV_UVLO_DIGITAL             (1U)
#define BITM_GFLTEV_UVLO_DIGITAL             (0X0010U)

#define BITP_GFLTEV_COMMAND_FAULT            (3U)          	/* Set if an invalid or disallowed command was sent by the host. */
#define BITL_GFLTEV_COMMAND_FAULT            (1U)
#define BITM_GFLTEV_COMMAND_FAULT            (0X0008U)

#define BITP_GFLTEV_PEC_FAULT                (2U)          	/* Set if a PEC fault has occurred during the SPI transaction. */
#define BITL_GFLTEV_PEC_FAULT                (1U)
#define BITM_GFLTEV_PEC_FAULT                (0X0004U)

#define BITP_GFLTEV_MEMORY_FAULT             (1U)          	/* Set if a fault has or faults have occurred in the memory. */
#define BITL_GFLTEV_MEMORY_FAULT             (1U)
#define BITM_GFLTEV_MEMORY_FAULT             (0X0002U)

#define BITP_GFLTEV_LOW_CKT_BRK_FAULT        (0U)          	/* Set if one or more circuit breakers are tripped in the return path. */
#define BITL_GFLTEV_LOW_CKT_BRK_FAULT        (1U)
#define BITM_GFLTEV_LOW_CKT_BRK_FAULT        (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          GFLTMSK                            Value			Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GFLTMSK_COMMAND_FAULT          (3U)          	/* If this bit is cleared, the command interrupt is cleared. */
#define BITL_GFLTMSK_COMMAND_FAULT          (1U)
#define BITM_GFLTMSK_COMMAND_FAULT          (0X0008U)

#define BITP_GFLTMSK_PEC_FAULT              (2U)          	/* If this bit is cleared, the PEC interrupt is cleared. */
#define BITL_GFLTMSK_PEC_FAULT              (1U)
#define BITM_GFLTMSK_PEC_FAULT              (0X0004U)

#define BITP_GFLTMSK_MEMORY_FAULT           (1U)          	/* If this bit is cleared, the memory interrupt is cleared. */
#define BITL_GFLTMSK_MEMORY_FAULT           (1U)
#define BITM_GFLTMSK_MEMORY_FAULT           (0X0002U)

#define BITP_GFLTMSK_LOW_CKT_BRK_FAULT      (0U)          	/* If this bit is cleared, the return path interrupt is cleared. */
#define BITL_GFLTMSK_LOW_CKT_BRK_FAULT      (1U)
#define BITM_GFLTMSK_LOW_CKT_BRK_FAULT      (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          GCAP                              Value         	Description
   ---------------------------------------------------------------------------------------------------- */

#define BITP_GCAP_SCCP_SUPPORT              (6U)         	/* Set to 1 if the SCCP is supported. */
#define BITL_GCAP_SCCP_SUPPORT              (1U)         	/* SCCP is not supported by LTC4296-1 without external UC. */
#define BITM_GCAP_SCCP_SUPPORT              (0X0040U)	
	
#define BITP_GCAP_WAKE_FWD_SUPPORT          (5U)         	/* Set to 1 if wake-up forwarding is supported. */
#define BITL_GCAP_WAKE_FWD_SUPPORT          (1U)	
#define BITM_GCAP_WAKE_FWD_SUPPORT          (0X0020U)	
	
#define BITP_GCAP_NUMPORTS                  (0U)         	/* Number of PSE ports. */
#define BITL_GCAP_NUMPORTS                  (5U)
#define BITM_GCAP_NUMPORTS                  (0X001FU)

/* ----------------------------------------------------------------------------------------------------
          GIOST                             Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GIOST_PG_OUT4                  (8U)           	/* Status of Port 4 power good. */
#define BITL_GIOST_PG_OUT4                  (1U)	
#define BITM_GIOST_PG_OUT4                  (0X0100U)	
	
#define BITP_GIOST_PG_OUT3                  (7U)           	/* Status of Port 3 power good. */
#define BITL_GIOST_PG_OUT3                  (1U)	
#define BITM_GIOST_PG_OUT3                  (0X0080U)	
	
#define BITP_GIOST_PG_OUT2                  (6U)           	/* Status of Port 2 power good. */
#define BITL_GIOST_PG_OUT2                  (1U)	
#define BITM_GIOST_PG_OUT2                  (0X0040U)	
	
#define BITP_GIOSTPG_OUT1                   (5U)           	/* Status of Port 1 power good. */
#define BITL_GIOSTPG_OUT1                   (1U)	
#define BITM_GIOSTPG_OUT1                   (0X0020U)	
	
#define BITP_GIOST_PG_OUT0                  (4U)           	/* Status of Port 0 power good. */
#define BITL_GIOST_PG_OUT0                  (1U)	
#define BITM_GIOST_PG_OUT0                  (0X0010U)	
	
#define BITP_GIOST_PAD_AUTO                 (3U)           	/* Status of AUTO pin. */
#define BITL_GIOST_PAD_AUTO                 (1U)	
#define BITM_GIOST_PAD_AUTO                 (0X0008U)	
	
#define BITP_GIOST_PAD_WAKEUP               (2U)           	/* Status of WAKEUP pin as driven by the host. */
#define BITL_GIOST_PAD_WAKEUP               (1U)	
#define BITM_GIOST_PAD_WAKEUP               (0X0004U)	
	
#define BITP_GIOST_PAD_WAKEUP_DRIVE         (1U)           	/* Status of WAKEUP pin as driven by the IC. */
#define BITL_GIOST_PAD_WAKEUP_DRIVE         (1U)
#define BITM_GIOST_PAD_WAKEUP_DRIVE         (0X0002U)

/* ----------------------------------------------------------------------------------------------------
          GCMD                          	Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GCMD_SW_RESET                  (8U)           	/* Software Reset. */
#define BITL_GCMD_SW_RESET                  (8U)
#define BITM_GCMD_SW_RESET                  (0XFF00U)

#define BITP_GCMD_WRITE_PROTECT             (0U)           	/* After writing the write unlock key (0x05), */
#define BITL_GCMD_WRITE_PROTECT             (8U)           	/* write access to all writeable registers is enabled. */
#define BITM_GCMD_WRITE_PROTECT             (0X00FFU)

/* ----------------------------------------------------------------------------------------------------
          GCFG                              Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GCFG_MASK_LOWFAULT             (5U)          	/* Write 1 to prevent ports from entering overload due to low-side faults. */
#define BITL_GCFG_MASK_LOWFAULT             (1U)
#define BITM_GCFG_MASK_LOWFAULT             (0X0020U)

#define BITP_GCFG_TLIM_DISABLE              (4U)          	/* Write 1 to disable tLIM timers of all ports. */
#define BITL_GCFG_TLIM_DISABLE              (1U)
#define BITM_GCFG_TLIM_DISABLE              (0X0010U)

#define BITP_GCFG_TLIM_TIMER_SLEEP          (2U)          	/* Configures the sleep regulator fault timer for all the ports */
#define BITL_GCFG_TLIM_TIMER_SLEEP          (2U)			/* and the deep sleep return path fault timer. */	
#define BITM_GCFG_TLIM_TIMER_SLEEP          (0X000CU)

#define BITP_GCFG_REFRESH                   (1U)          	/* Write 1 to copy contents from non-volatile memory into volatile memory. */
#define BITL_GCFG_REFRESH                   (1U)			/* Auto cleared after completion. */
#define BITM_GCFG_REFRESH                   (0X0002U)

#define BITP_GCFG_SW_VIN_PGOOD              (0U)          	/* Write 1 to indicate that the system is ready to source */
#define BITL_GCFG_SW_VIN_PGOOD              (1U)			/* the required power to connected PD(s). */
#define BITM_GCFG_SW_VIN_PGOOD              (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          GADCCFG                           Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GADC_SAMPLE_MODE               (5U)          	/* Configures global ADC. */
#define BITL_GADC_SAMPLE_MODE               (2U)	
#define BITM_GADC_SAMPLE_MODE               (0X0060U)	
	
#define BITP_GADC_SEL                       (0U)          	/* Configures global ADC inputs. */
#define BITL_GADC_SEL                       (5U)
#define BITM_GADC_SEL                       (0X001FU)

/* ----------------------------------------------------------------------------------------------------
          GADCDAT                           Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_GADC_MISSED                    (13U)        	/* Configures global ADC inputs. */
#define BITL_GADC_MISSED                    (1U)	
#define BITM_GADC_MISSED                    (0X2000U)	
	
#define BITP_GADC_NEW                       (12U)        	/* Configures global ADC. */
#define BITL_GADC_NEW                       (1U)	
#define BITM_GADC_NEW                       (0X1000U)	
	
#define BITP_GADC                           (0U)         	/* Global ADC Accumulation Result. */
#define BITL_GADC                           (12U)
#define BITM_GADC                           (0X0FFFU)

/* ----------------------------------------------------------------------------------------------------
          PxEV                           	Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_VALID_SIGNATURE				(9U)           	/* Set when a valid signature was detected on the port. */
#define BITL_VALID_SIGNATURE                (1U)	
#define BITM_VALID_SIGNATURE                (0X0200U)	
	
#define BITP_INVALID_SIGNATURE              (8U)           	/* Set when an invalid signature was detected on the port. */
#define BITL_INVALID_SIGNATURE              (1U)	
#define BITM_INVALID_SIGNATURE              (0X0100U)	
	
#define BITP_TOFF_TIMER_DONE                (7U)           	/* Set when the tOFF timer expired when the port was discharging toward VSLEEP. */
#define BITL_TOFF_TIMER_DONE                (1U)	
#define BITM_TOFF_TIMER_DONE                (0X0080U)	
	
#define BITP_OVERLOAD_DETECTED_ISLEEP       (6U)           	/* Set when the overload timer tLIM expired due to overcurrent. */
#define BITL_OVERLOAD_DETECTED_ISLEEP       (1U)           	/* while the port was attempting to apply VSLEEP at the power interface. */
#define BITM_OVERLOAD_DETECTED_ISLEEP       (0X0040U)	
	
#define BITP_OVERLOAD_DETECTED_IPOWERED     (5U)           	/* Set when the overload timer tLIM expired due to overcurrent while the port was in the power-up or power-on state. */
#define BITL_OVERLOAD_DETECTED_IPOWERED     (1U)
#define BITM_OVERLOAD_DETECTED_IPOWERED		(0X0020U)

#define BITP_MFVS_TIMEOUT                   (4U)          	/* Set when power was removed due to tMFVDO timer expiration. */
#define BITL_MFVS_TIMEOUT                   (1U)	
#define BITM_MFVS_TIMEOUT                   (0X0010U)	
	
#define BITP_TINRUSH_TIMER_DONE             (3U)          	/* Set when tINRUSH timer expired when the port was in the power-up state. */
#define BITL_TINRUSH_TIMER_DONE             (1U)	
#define BITM_TINRUSH_TIMER_DONE             (0X0008U)	
	
#define BITP_PD_WAKEUP                      (2U)          	/* Set when an upstream (PD initiated) wake-up is detected. */
#define BITL_PD_WAKEUP                      (1U)	
#define BITM_PD_WAKEUP                      (0X0004U)	
	
#define BITP_LSNS_FORWARD_FAULT             (2U)          	/* Set when low-side forward circuit breaker fault event has occurred on the port. */
#define BITL_LSNS_FORWARD_FAULT             (1U)	
#define BITM_LSNS_FORWARD_FAULT             (0X0002U)	
	
#define BITP_LSNS_REVERSE_FAULT             (1U)          	/* Set when low-side reverse circuit breaker fault event has occurred on the port. */
#define BITL_LSNS_REVERSE_FAULT             (1U)
#define BITM_LSNS_REVERSE_FAULT             (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          PxST                              Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_DET_VHIGH                      (13U)           /* Set when port voltage is greater than VBAD_HI_PSE during detection. */
#define BITL_DET_VHIGH                      (1U)
#define BITM_DET_VHIGH                      (0X2000U)

#define BITP_DET_VLOW                       (12U)           /* Set when port voltage is less than VBAD_LO_PSE during detection. */
#define BITL_DET_VLOW                       (1U)
#define BITM_DET_VLOW                       (0X1000U)

#define BITP_POWER_STABLE_HI                (11U)           /* Set when following inrush port is sourcing the full operating voltage. */
#define BITL_POWER_STABLE_HI                (1U)
#define BITM_POWER_STABLE_HI                (0X0800U)

#define BITP_POWER_STABLE_LO                (10U)           /* Set when following inrush port is sourcing the full operating voltage. */
#define BITL_POWER_STABLE_LO                (1U)
#define BITM_POWER_STABLE_LO                (0X0400U)

#define BITP_POWER_STABLE                   (9U)          	/* Set when the port is delivering the full operating voltage to the output. */
#define BITL_POWER_STABLE                   (1U)	
#define BITM_POWER_STABLE                   (0X0200U)	
	
#define BITP_OVERLOAD_HELD                  (8U)          	/* Set when the port is in the overload state. */
#define BITL_OVERLOAD_HELD                  (1U)	
#define BITM_OVERLOAD_HELD                  (0X0100U)	
	
#define BITP_PI_SLEEPING                    (7U)          	/* Set when the port is in the settle-sleep or sleep state. */
#define BITL_PI_SLEEPING                    (1U)	
#define BITM_PI_SLEEPING                    (0X0080U)	
	
#define BITP_PI_PREBIASED                   (6U)          	/* Set when the port is in the idle state. */
#define BITL_PI_PREBIASED                   (1U)	
#define BITM_PI_PREBIASED                   (0X0040U)	
	
#define BITP_PI_DETECTING                   (5U)          	/* Set when the port is in the detection state. */
#define BITL_PI_DETECTING                   (1U)	
#define BITM_PI_DETECTING                   (0X0020U)	
	
#define BITP_PI_POWERED                     (4U)          	/* Set when the port is in the power-up or power-on state. */
#define BITL_PI_POWERED                     (1U)
#define BITM_PI_POWERED                     (0X0010U)

#define BITP_PI_DISCHARGE_EN                (3U)           	/* Set when the port is in the settle-sleep state. */
#define BITL_PI_DISCHARGE_EN                (1U)
#define BITM_PI_DISCHARGE_EN                (0X0008U)

#define BITP_PSE_STATUS                     (0U)           	/* PSE status. */
#define BITL_PSE_STATUS                     (3U)
#define BITM_PSE_STATUS                     (0X0007U)

/* ----------------------------------------------------------------------------------------------------
          PxCFG0                            Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_SW_INRUSH                      (15U)           /* Write 1 to skip prebias and detection and directly power up the PD. */
#define BITL_SW_INRUSH                      (1U)
#define BITM_SW_INRUSH                      (0X8000U)

#define BITP_END_CLASSIFICATION             (14U)           /* Write 1 to end classification. Autocleared by the IC. */
#define BITL_END_CLASSIFICATION             (1U)
#define BITM_END_CLASSIFICATION             (0X4000U)

#define BITP_SET_CLASSIFICATION_MODE        (13U)           /* Write 1 to set the port in classification mode. */
#define BITL_SET_CLASSIFICATION_MODE        (1U)
#define BITM_SET_CLASSIFICATION_MODE        (0X2000U)

#define BITP_DISABLE_DETECTION_PULLUP       (12U)           /* Write 1 to disable the detection pull-up curren. */
#define BITL_DISABLE_DETECTION_PULLUP       (1U)
#define BITM_DISABLE_DETECTION_PULLUP       (0X1000U)

#define BITP_TDET_DISABLE                   (11U)           /* Write 1 to disable the detection timer. */
#define BITL_TDET_DISABLE                   (1U)
#define BITM_TDET_DISABLE                   (0X0800U)

#define BITP_FOLDBACK_DISABLE               (10U)           /* Write 1 to disable foldback during port inrush in the power-up state. */
#define BITL_FOLDBACK_DISABLE               (1U)
#define BITM_FOLDBACK_DISABLE               (0X0400U)

#define BITP_SOFT_START_DISABLE             (9U)           	/* Write 1 to disable soft-start during port inrush in the power-up state. */
#define BITL_SOFT_START_DISABLE             (1U)
#define BITM_SOFT_START_DISABLE             (0X0200U)

#define BITP_TOFF_TIMER_DISABLE             (8U)           	/* Write 1 to disable the tOFF timer to allow the port arbitrarily long time for discharging in the settle-sleep state. */
#define BITL_TOFF_TIMER_DISABLE             (1U)
#define BITM_TOFF_TIMER_DISABLE             (0X0100U)

#define BITP_TMFVDO_TIMER_DISABLE           (7U)          	/* Write 1 to disable the tMFVDO timer to prevent the port from shutting off in absence of a valid MFVS. */
#define BITL_TMFVDO_TIMER_DISABLE           (1U)	
#define BITM_TMFVDO_TIMER_DISABLE           (0X0080U)	
	
#define BITP_SW_PSE_READY                   (6U)          	/* Write 1 to indicate that the port is able to source power to the connected PD */
#define BITL_SW_PSE_READY                   (1U)	
#define BITM_SW_PSE_READY                   (0X0040U)	
	
#define BITP_SW_POWER_AVAILABLE             (5U)          	/* Write 1 to indicate that the port is able to source power to the connected PD. */
#define BITL_SW_POWER_AVAILABLE             (1U)	
#define BITM_SW_POWER_AVAILABLE             (0X0020U)	
	
#define BITP_UPSTREAM_WAKEUP_DISABLE        (4U)          	/* Write 1 to disable the upstream (PD initiated) wake-up of the port. */
#define BITL_UPSTREAM_WAKEUP_DISABLE        (1U)	
#define BITM_UPSTREAM_WAKEUP_DISABLE        (0X0010U)	
	
#define BITP_DOWNSTREAM_WAKEUP_DISABLE      (3U)          	/* Write 1 to disable the downstream (PSE initiated) wake-up of the port. */
#define BITL_DOWNSTREAM_WAKEUP_DISABLE      (1U)	
#define BITM_DOWNSTREAM_WAKEUP_DISABLE		(0X0008U)	
	
#define BITP_SW_PSE_WAKEUP                  (2U)          	/* Write 1 to wake up the port. */
#define BITL_SW_PSE_WAKEUP                  (1U)	
#define BITM_SW_PSE_WAKEUP                  (0X0004U)	
	
#define BITP_HW_EN_MASK                     (1U)          	/* Write 0 to mask the AUTO pin. */
#define BITL_HW_EN_MASK                     (1U)	
#define BITM_HW_EN_MASK                     (0X0002U)	
	
#define BITP_SW_EN                          (0U)          	/* Write 1 to enable the port. */
#define BITL_SW_EN                          (1U)
#define BITM_SW_EN                          (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          PxCFG1                            Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_PREBIAS_OVERRIDE_GOOD          (8U)           	/* Write 1 to simulate a valid wake-up signature. */
#define BITL_PREBIAS_OVERRIDE_GOOD          (1U)
#define BITM_PREBIAS_OVERRIDE_GOOD          (0X0100U)

#define BITP_TLIM_TIMER_TOP                 (6U)           	/* Write as follows to configure the top-side fault timer (tLIM). */
#define BITL_TLIM_TIMER_TOP                 (2U)	
#define BITM_TLIM_TIMER_TOP                 (0X00C0U)	
	
#define BITP_TOD_TRESTART_TIMER             (4U)           	/* Set when the port is in the settle-sleep state. */
#define BITL_TOD_TRESTART_TIMER             (2U)	
#define BITM_TOD_TRESTART_TIMER             (0X0030U)	
	
#define BITP_TINRUSH_TIMER                  (2U)           	/* Write as follows to configure the overload delay timer and restart timer. */
#define BITL_TINRUSH_TIMER                  (2U)	
#define BITM_TINRUSH_TIMER                  (0X000CU)	
	
#define BITP_SIG_OVERRIDE_BAD               (1U)           	/* Write 1 to simulate an invalid detection voltage signature. */
#define BITL_SIG_OVERRIDE_BAD               (1U)	
#define BITM_SIG_OVERRIDE_BAD               (0X0002U)	
	
#define BITP_SIG_OVERRIDE_GOOD              (0U)           	/* Write 1 to simulate a valid detection voltage signature. */
#define BITL_SIG_OVERRIDE_GOOD              (1U)
#define BITM_SIG_OVERRIDE_GOOD              (0X0001U)

/* ----------------------------------------------------------------------------------------------------
          PxADCCFG                          Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_MFVS_THRESHOLD                 (0U)           	/* MFVS threshold to be set based on the MFVS Threshold Code = 62.5 × R1 equation. */
#define BITL_MFVS_THRESHOLD                 (8U)
#define BITM_MFVS_THRESHOLD                 (0X00FFU)

/* ----------------------------------------------------------------------------------------------------
          PxADCDAT                          Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_MISSED                         (13U)           /* Set when the host has missed one or more measurements. */
#define BITL_MISSED                         (1U)
#define BITM_MISSED                         (0X2000U)

#define BITP_NEW                            (12U)           /* Set when a new measurement result of source current. */
#define BITL_NEW                            (1U)
#define BITM_NEW                            (0X1000U)

#define BITP_SOURCE_CURRENT                 (0U)           	/* Source current measurement. */
#define BITL_SOURCE_CURRENT                 (12U)
#define BITM_SOURCE_CURRENT                 (0X0FFFU)

/* ----------------------------------------------------------------------------------------------------
          PxSELFTEST                        Value           Description
   ---------------------------------------------------------------------------------------------------- */
#define BITP_FORCE_BAD_OUTM                 (1U)           	/* Write 1 to simulate a power bad on OUTMx pin of the port. */
#define BITL_FORCE_BAD_OUTM                 (1U)
#define BITM_FORCE_BAD_OUTM                 (0X0002U)

#define BITP_FORCE_BAD_OUTP                 (0U)           	/* Write 1 to simulate a power bad on OUTPx pin of the port. */
#define BITL_FORCE_BAD_OUTP                 (1U)
#define BITM_FORCE_BAD_OUTP                 (0X0001U)

/*   ---------------------------------------------------------------------------------------------------- */

/**@}*/

#endif /* LTC4296_1_REG_DEFINES_H_ */



