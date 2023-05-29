namespace GameBoyEmulator.Core
{
    public static class Gpu
    {
        private static byte _control;
        private static byte _scrollX;
        private static byte _scrollY;
        private static ulong _lastTicks;
        private static ulong _tick;
        
        public static void Step()
        {
            _tick += Clock.Cycle - _lastTicks;
            _lastTicks = Clock.Cycle;
	
            switch(Ram.STAT_Mode) {
                case Modes.HBlank:
                    if(_tick >= 204) {
                        //hblank(); - This only increments the scanline so instead I've just done this on the next line for now
                        Ram.LY++;
				
                        if(Ram.LY == 143) {
                            //if(interrupt.enable & INTERRUPTS_VBLANK) interrupt.flags |= INTERRUPTS_VBLANK;
					
                            Ram.STAT_Mode = Modes.VBlank;
                        }
                        else Ram.STAT_Mode = Modes.Oam;
				
                        _tick -= 204;
                    }
                    break;
		
                case Modes.VBlank:
                    if(_tick >= 456) {
                        Ram.LY++;
				
                        if(Ram.LY > 153) {
                            Ram.LY = 0;
                            Ram.STAT_Mode = Modes.Oam;
                        }
				
                        _tick -= 456;
                    }
                    break;

                case Modes.Oam:
                    if(_tick >= 80) {
                        Ram.STAT_Mode = Modes.Vram;
                        _tick -= 80;
                    }
                    break;
                case Modes.Vram:
                    if(_tick >= 172) {
                        Ram.STAT_Mode = Modes.HBlank;
                        _tick -= 172;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}