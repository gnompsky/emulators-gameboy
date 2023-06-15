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
        private readonly PixelFifo _backgroundFifo;

        private State _state = State.GetTile;
        private byte _curTileIndex;
        private byte _curTileDataLow;
        private byte _curTileDataHigh;
        private int _curTileX = 0;
        private int _curTileY = 0;

        public PixelFetcher(LcdHandler lcd, VRamHandler vRam, OamHandler oam, PixelFifo backgroundFifo)
        {
            _lcd = lcd;
            _vRam = vRam;
            _oam = oam;
            _backgroundFifo = backgroundFifo;
        }

        public void Step(int tick)
        {
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
                    if (TryPush())
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

        private bool TryPush()
        {
            if (_backgroundFifo.Count > 8) return false;
            
            for (var bit = 0; bit < 8; bit++)
            {
                var color = (Colors)(
                    (_curTileDataLow.BitIsSet(bit) ? 0b10 : 0b00) |
                    (_curTileDataHigh.BitIsSet(bit) ? 0b01 : 0b00)
                );

                var palette = Pixel.Palettes.OBP0;
                var backgroundPriority = Pixel.BackgroundPriorities.ObjFirst;
                if (color != Colors.Black) Console.WriteLine("Enqueueing " + color);
                _backgroundFifo.Enqueue(new Pixel(color, palette, backgroundPriority));
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