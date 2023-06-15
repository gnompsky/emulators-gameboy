using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.Components
{
    public class PPU
    {
        public readonly Fifo<Colors> LcdFifo = new Fifo<Colors>();
        private readonly Fifo<Colors> _backgroundFifo = new Fifo<Colors>();
        private readonly Fifo<ObjPixel> _oamFifo = new Fifo<ObjPixel>();
        
        private readonly InterruptHandler _interrupts;
        private readonly LcdHandler _lcd;

        private readonly PixelFetcher _pixelFetcher;

        private int _tick;

        public PPU(Memory memory)
        {
            _interrupts = memory.InterruptHandler;
            _lcd = memory.LcdHandler;

            _pixelFetcher = new PixelFetcher(
                _lcd,
                memory.VRamHandler,
                memory.OamHandler,
                _backgroundFifo,
                _oamFifo,
                LcdFifo
            );
        }

        public void Step(int cyclesTaken)
        {
            _tick += cyclesTaken;

            switch(_lcd.STATMode) {
                case Modes.HBlank:
                    if(_tick >= 204) {
                        //hblank(); - This only increments the scanline so instead I've just done this on the next line for now
                        _lcd.LY++;
				
                        if(_lcd.LY == 143) {
                            _interrupts.RequestInterrupt(InterruptHandler.Interrupt.VBlank);
					
                            _lcd.STATMode = Modes.VBlank;
                        }
                        else _lcd.STATMode = Modes.Oam;
				
                        _tick -= 204;
                    }
                    break;
		
                case Modes.VBlank:
                    if(_tick >= 456) {
                        _lcd.LY++;
				
                        if(_lcd.LY > 153) {
                            _lcd.LY = 0;
                            _lcd.STATMode = Modes.Oam;
                        }
				
                        _tick -= 456;
                    }
                    break;

                case Modes.Oam:
                    if(_tick >= 80) {
                        _lcd.STATMode = Modes.Vram;
                        _backgroundFifo.Clear();
                        _oamFifo.Clear();
                        _tick -= 80;
                    }
                    break;
                case Modes.Vram:
                    _pixelFetcher.Step(ref _tick);
                    if(_tick >= 172) {
                        _lcd.STATMode = Modes.HBlank;
                        _tick -= 172;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}