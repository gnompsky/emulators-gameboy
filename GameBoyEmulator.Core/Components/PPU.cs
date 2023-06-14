using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.Components
{
    public class PPU
    {
        public readonly PixelFifo BackgroundFifo = new PixelFifo();
        public readonly PixelFifo OamFifo = new PixelFifo();
        
        private readonly InterruptHandler _interrupts;
        private readonly LcdHandler _lcd;

        private readonly PixelFetcher _pixelFetcher;

        private int _tick;

        public PPU(Memory memory)
        {
            _interrupts = memory.InterruptHandler;
            _lcd = memory.LcdHandler;

            _pixelFetcher = new PixelFetcher(_lcd, memory.VRamHandler, memory.OamHandler, BackgroundFifo);
        }

        public void Step(int cyclesTaken)
        {
            _tick += cyclesTaken;

            switch(_lcd.STAT_Mode) {
                case Modes.HBlank:
                    if(_tick >= 204) {
                        //hblank(); - This only increments the scanline so instead I've just done this on the next line for now
                        _lcd.LY++;
				
                        if(_lcd.LY == 143) {
                            _interrupts.RequestInterrupt(InterruptHandler.Interrupt.VBlank);
					
                            _lcd.STAT_Mode = Modes.VBlank;
                        }
                        else _lcd.STAT_Mode = Modes.Oam;
				
                        _tick -= 204;
                    }
                    break;
		
                case Modes.VBlank:
                    if(_tick >= 456) {
                        _lcd.LY++;
				
                        if(_lcd.LY > 153) {
                            _lcd.LY = 0;
                            _lcd.STAT_Mode = Modes.Oam;
                        }
				
                        _tick -= 456;
                    }
                    break;

                case Modes.Oam:
                    if(_tick >= 80) {
                        _lcd.STAT_Mode = Modes.Vram;
                        BackgroundFifo.Clear();
                        OamFifo.Clear();
                        _tick -= 80;
                    }
                    break;
                case Modes.Vram:
                    _pixelFetcher.Step(_tick);
                    if(_tick >= 172) {
                        _lcd.STAT_Mode = Modes.HBlank;
                        _tick -= 172;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}