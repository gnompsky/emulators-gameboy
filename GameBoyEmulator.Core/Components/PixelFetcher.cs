using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.Components
{
    public class PixelFetcher
    {
        private readonly LcdHandler _lcd;
        private readonly VRamHandler _vRam;
        private readonly OamHandler _oam;
        private readonly Fifo<Colors> _backgroundFifo;
        private readonly Fifo<ObjPixel> _oamFifo;
        private readonly Fifo<Colors> _finalFifo;

        private State _state = State.GetTile;
        private byte _curTileIndex;
        private byte _curTileDataLow;
        private byte _curTileDataHigh;
        private int _curTileX = 0;
        private int _curTileY = 0;

        public PixelFetcher(
            LcdHandler lcd, 
            VRamHandler vRam, 
            OamHandler oam, 
            Fifo<Colors> backgroundFifo, 
            Fifo<ObjPixel> oamFifo,
            Fifo<Colors> finalFifo
        )
        {
            _lcd = lcd;
            _vRam = vRam;
            _oam = oam;
            _backgroundFifo = backgroundFifo;
            _oamFifo = oamFifo;
            _finalFifo = finalFifo;
        }

        public void Step(ref int tick)
        {
            if (_lcd.LCDCWindowEnabled)
            {
                // TODO: This will likely break unless my timing is good...which it's not
                _backgroundFifo.Clear();
                _state = State.GetTile;
                if (_lcd.WX == 0x00 && (_lcd.SCX & 7) > 0)
                {
                    tick++; // Shorten by 1 dot - we do this by pretending we're 1 dot further on than we are
                }
            }
            
            switch (_state)
            {
                case State.GetTile:
                    _curTileIndex = GetTileIndex();
                    _state = State.GetTileDataLow;
                    break;
                case State.GetTileDataLow:
                    _curTileDataLow = GetTileData(_lcd.LCDCBgAndWindowTileDataAreaUsesSignedAddress, false);
                    _state = State.GetTileDataHigh;
                    break;
                case State.GetTileDataHigh:
                    _curTileDataHigh = GetTileData(_lcd.LCDCBgAndWindowTileDataAreaUsesSignedAddress, true);
                    // TODO: This also pushes a row of background/window pixels to the FIFO.
                    // This extra push is not part of the 8 steps, meaning there’s 3 total chances to push pixels to
                    // the background FIFO every time the complete fetcher steps are performed.
                    _state = State.Sleep;
                    break;
                case State.Sleep:
                    _state = State.Push;
                    break;
                case State.Push:
                    if (TryPushBg())
                    {
                        // If we weren't able to push, try again later
                        _state = State.GetTile;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private byte GetTileIndex()
        {
            var tileMapAddress = _lcd.LCDCBgTileMapArea;
            var isWindow = _lcd.LCDCWindowEnabled;
            
            var fetcherX = isWindow
                ? _lcd.WX
                : (byte)(((_lcd.SCX / 8) + _curTileX) & 0x1F);
            var fetcherY = isWindow
                ? _lcd.WY
                : (byte)((_lcd.LY + _lcd.SCY) & 0xFF);

            return _vRam.ReadValue((ushort)(tileMapAddress + fetcherX + fetcherY / 8 * 32));
        }

        private byte GetTileData(bool isSignedAddressMode, bool isHighByte)
        {
            var addressBase = _lcd.LCDCBgAndWindowTileDataArea(isSignedAddressMode);
            var tileLowAddress = (ushort)(
                addressBase +
                (isSignedAddressMode ? unchecked((sbyte)_curTileIndex) : _curTileIndex)
            );
            var tileDataAddress = isHighByte ? (ushort)(tileLowAddress + 1) : tileLowAddress;
            return _vRam.ReadValue(tileDataAddress);
        }

        private bool TryPushBg()
        {
            if (_backgroundFifo.Count > 8) return false;

            var palette = _lcd.BGP;
            
            for (var bit = 0; bit < 8; bit++)
            {
                var colorIndex = (
                    (_curTileDataLow.BitIsSet(bit) ? 0b10 : 0b00) |
                    (_curTileDataHigh.BitIsSet(bit) ? 0b01 : 0b00)
                );
                var color = palette[colorIndex];

                _backgroundFifo.Enqueue(color);
            }

            return true;
        }

        private enum State
        {
            GetTile,
            GetTileDataLow,
            GetTileDataHigh,
            Sleep,
            Push
        }
    }
}