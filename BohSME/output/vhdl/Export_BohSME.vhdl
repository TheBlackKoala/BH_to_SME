library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.NUMERIC_STD.ALL;

-- library SYSTEM_TYPES;
use work.SYSTEM_TYPES.ALL;

-- library CUSTOM_TYPES;
use work.CUSTOM_TYPES.ALL;

-- User defined packages here
-- #### USER-DATA-IMPORTS-START
-- #### USER-DATA-IMPORTS-END

entity BohSME_export is
  port(

    -- Top-level bus tdata signals
    tdata_1_val: in STD_LOGIC_VECTOR(31 downto 0);

    -- Top-level bus tdata signals
    tdata_3_val: out STD_LOGIC_VECTOR(31 downto 0);


    -- User defined signals here
    -- #### USER-DATA-ENTITYSIGNALS-START
    -- #### USER-DATA-ENTITYSIGNALS-END


    -- Enable signal
    ENB : in STD_LOGIC;

	-- Reset signal
    RST : in STD_LOGIC;

    -- Finished signal
    FIN : out Std_logic;

    -- Clock signal
    CLK : in STD_LOGIC
  );
end BohSME_export;



architecture RTL of BohSME_export is  
  -- User defined signals here
  -- #### USER-DATA-SIGNALS-START
  -- #### USER-DATA-SIGNALS-END

  -- Intermediate conversion signal to convert internal types to external ones
  signal tmp_tdata_3_val : T_SYSTEM_INT32;

begin

    -- Carry converted signals from entity to wrapped outputs
  tdata_3_val <= std_logic_vector(tmp_tdata_3_val);

    -- Entity BohSME signals
    BohSME: entity work.BohSME
    port map (
        -- Input bus tdata
        tdata_1_val => signed(tdata_1_val),

        -- Output bus tdata
        tdata_3_val => tmp_tdata_3_val,

        ENB => ENB,
        RST => RST,
        FIN => FIN,
        CLK => CLK
    );

-- User defined processes here
-- #### USER-DATA-CODE-START
-- #### USER-DATA-CODE-END

end RTL;