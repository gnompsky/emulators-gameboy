using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.Components
{
    public class PPU
    {
        private readonly LcdHandler _lcd;

        private byte _control;
        private byte _scrollX;
        private byte _scrollY;
        private ulong _lastTicks;
        private ulong _tick;

        public PPU(Memory memory)
        {
            _lcd = memory.LcdHandler;
        }
        
        public void Step()
        {
            _tick += Clock.Cycle - _lastTicks;
            _lastTicks = Clock.Cycle;
	
            switch(_lcd.STAT_Mode) {
                case Modes.HBlank:
                    if(_tick >= 204) {
                        //hblank(); - This only increments the scanline so instead I've just done this on the next line for now
                        _lcd.LY++;
				
                        if(_lcd.LY == 143) {
                            //if(interrupt.enable & INTERRUPTS_VBLANK) interrupt.flags |= INTERRUPTS_VBLANK;
					
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
                        _tick -= 80;
                    }
                    break;
                case Modes.Vram:
                    if(_tick >= 172) {
                        _lcd.STAT_Mode = Modes.HBlank;
                        _tick -= 172;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: Debug print, fills LY rows of pixels white 
            var bytes = new byte[256 * 256];
            Array.Fill(bytes, (byte)0x0, 0, bytes.Length);
            Array.Fill(bytes, (byte)0x1, 0, _lcd.LY * 256);

            //GpuPixelsUpdated?.Invoke(bytes);
        }
    }
}