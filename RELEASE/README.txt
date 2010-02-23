You need the .Net 4 RC framework to run this!  If you run into problems (crashes, bnet module won't activate, etc) contact "Sagu" on rizon or email deeplogic.dev@gmail.com

To run, start "DLS_Launcher.exe"
BitStreamEditor has been compiled as an executable for offline use.  It's normally used within the ExpConsole in the framework.

Instructions to get to the ExpConsole
1. Wait for the framework to initialize
2. Login by pressing Enter (Create an account if you haven't yet)
3. Press "c" and hit enter
4. type "help" and hit enter
5. Type an alias of the plugin module you want to use.


-------------------------------------------------------
ExpConsole Sample (IP has been changed to a random one)
-------------------------------------------------------

Welcome to the DeepLogic Experimental Console Service

71.26.115.42:~$ help

Module: DeepLogic.Core.ExpConsole.Modules.Database
Aliases: Database, db
Upgrades database objects to the stored ModernVersion.
    Usage: dbupgrade [db1,db2,db..]
        Note: All databases are selected if none is specified

        Param: db [Optional] - Specify the database to upgrade:
          Identity
          Settings
          Users
          Security
          DataGroup
          DataPool

Module: BNet.Plugin.ConsoleModules.BNetExpConsole
Aliases: BNet, battlenet
BNet Plugin: Stub
    Console Commands Not Implemented

Module: BNet.Plugin.ConsoleModules.BitStreamExpConsole
Aliases: BitStream, bs
BitStream Plugin: Analyze/Inject packet bitstreams

  Commands:
    Analyzer {On, Off} - Spawns a bitstream analyzer window for received packets (Warning: Controlled traffic is advised)

    InjectPacket {[HEX DATA]} - Broadcasts a packet to all connected clients
                                If no HEX data is present, a bitstream writer window will be shown

    QueuePacket {[HEX DATA]} - Suspends Packet Processing and queues a packet response for the next client that sends data
                               Supports enqueuing of more than one packet. The packet at the front of the queue will be sent
                               If no HEX data is present, a bitstream writer window will be shown


71.26.115.42:~$ bs analyzer on
New packets will show up in the BitStream Analyzer window.  Use "Analyze Off" to disable.

71.26.115.42:~$ bs
71.26.115.42:BitStream$ analyzer off
Stream inspector removed from the socket server.

71.26.115.42:BitStream$ queuepacket 48656C6C6F20776F726C6421010203040506070809101112131415161718192021222324252627282930313233343536373839404142
Packet Queued (1 in queue)
[BitStream]
[0]:{01001000011001010110110001101100}
[1]:{01101111001000000111011101101111}
[2]:{01110010011011000110010000100001}
[3]:{00000001000000100000001100000100}
[4]:{00000101000001100000011100001000}
[5]:{00001001000100000001000100010010}
[6]:{00010011000101000001010100010110}
[7]:{00010111000110000001100100100000}
[8]:{00100001001000100010001100100100}
[9]:{00100101001001100010011100101000}
[10]:{00101001001100000011000100110010}
[11]:{00110011001101000011010100110110}
[12]:{00110111001110000011100101000000}
[13]:{0100000101000010................}

[Hex]
48 65 6C 6C 6F 20 77 6F 72 6C 64 21 01 02 03 04     Hello world!....
05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20     ...............
21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36     !"#$%&'()0123456
37 38 39 40 41 42                                   789@AB



71.26.115.42:BitStream$ exit
71.26.115.42:~$
