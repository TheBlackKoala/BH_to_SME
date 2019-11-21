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

entity BohSME is
  port(


    -- Top-level bus tdata signals
    tdata_2_val: out T_SYSTEM_INT32;


    -- Interconnection bus tdata signals
    tdata_1_val: inout T_SYSTEM_INT32;
    -- Interconnection bus tdata signals
    tdata_0_val: inout T_SYSTEM_INT32;

    -- User defined signals here
    -- #### USER-DATA-ENTITYSIGNALS-START
    -- #### USER-DATA-ENTITYSIGNALS-END

    -- Enable signal
    ENB : in Std_logic;

    -- Finished signal
    FIN : out Std_logic;

	-- Reset signal
    RST : in Std_logic;

	-- Clock signal
	CLK : in Std_logic
  );
end BohSME;

architecture RTL of BohSME is  
    -- User defined signals here
    -- #### USER-DATA-SIGNALS-START
    -- #### USER-DATA-SIGNALS-END


    -- Process ready triggers

    signal FIN_instr0 : std_logic;

    signal FIN_instr1 : std_logic;

    signal FIN_Creater : std_logic;


    -- The primary ready driver signal
    signal RDY : std_logic;

begin


    -- Entity  instr0 signals
    instr0: entity work.instr0
    port map (
        -- Input bus tdata
        a1_val => tdata_1_val,


        -- Output bus tdata
        a0_val => tdata_0_val,



        CLK => CLK,
        RDY => RDY,
        FIN => FIN_instr0,
        ENB => ENB,
        RST => RST
    );


    -- Entity  instr1 signals
    instr1: entity work.instr1
    port map (
        -- Input bus tdata
        a0_val => tdata_0_val,


        -- Output bus tdata
        a2_val => tdata_2_val,



        CLK => CLK,
        RDY => RDY,
        FIN => FIN_instr1,
        ENB => ENB,
        RST => RST
    );


    -- Entity  Creater signals
    Creater: entity work.Creater
    generic map(
        reset_a => TO_SIGNED(0, 32),
        reset_len => TO_SIGNED(9, 32),
        reset_data => (TO_SIGNED(0, 32), TO_SIGNED(1, 32), TO_SIGNED(2, 32), TO_SIGNED(3, 32), TO_SIGNED(4, 32), TO_SIGNED(5, 32), TO_SIGNED(6, 32), TO_SIGNED(7, 32), others => TO_SIGNED(8, 32))
    )
    port map (
        -- Output bus tdata
        a1_val => tdata_1_val,



        CLK => CLK,
        RDY => RDY,
        FIN => FIN_Creater,
        ENB => ENB,
        RST => RST
    );


    -- Connect ready signals

    -- Setup the FIN feedback signals
    process(
      FIN_instr0, 
      FIN_instr1, 
      FIN_Creater
    )
    begin
      if FIN_instr0 = FIN_instr1 AND FIN_instr0 = FIN_Creater then
        FIN <= FIN_instr0;
      end if;
    end process;

    -- Propagate all clocked and feedback signals
    process(
        CLK,
        RST)

        variable readyflag: std_logic;
    begin
        if RST = '1' then
            RDY <= '0';
            readyflag := '1';
        elsif rising_edge(CLK) then
            if ENB = '1' then
                readyflag := not readyflag;
                RDY <= readyflag;


            end if;
        end if;
    end process;



-- User defined processes here
-- #### USER-DATA-CODE-START
-- #### USER-DATA-CODE-END

end RTL;