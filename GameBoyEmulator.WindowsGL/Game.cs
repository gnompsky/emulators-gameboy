using GameBoyEmulator.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBoyEmulator.WindowsGL
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static Game Instance { get; private set; }
        public static Viewport Viewport => Instance.GraphicsDevice.Viewport;
        public static BasicEffect BasicEffect;
        public static Texture2D GeneratedTexture;
        public static Vector3 camera2DScrollPosition = new Vector3(0, 0, -1);
        public static Vector3 camera2DScrollLookAt = new Vector3(0, 0, 0);
        public static float camera2DrotationZ = 0f;
        
        private readonly GraphicsDeviceManager _graphics;
        private const int BUFFER_W = 256;
        private const int BUFFER_H = 256;
        private readonly byte[] _pixelBuffer = new byte[BUFFER_W * BUFFER_H];

        private bool _pixelsDirty = true;
        private string _lastInstructionName = "";
        private bool _running = true;

        private Thread _emulatorThread = new Thread(() =>
        {
            while (Instance._running)
            {
                Cpu.Step();
                Gpu.Step();
                //DebugPrint();
            }
        });

        public Game()
        {
            Instance = this;
            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = BUFFER_W;
            _graphics.PreferredBackBufferHeight = BUFFER_H;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            Cpu.Reset();

            var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/cpu_instrs.gb");
            //var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/Tetris (World) (Rev 1).gb");
            Ram.LoadROM(romBytes);

            Instructions.InstructionExecuting += name => _lastInstructionName = name;
            Gpu.GpuPixelsUpdated += bytes =>
            {
                Array.Copy(bytes, _pixelBuffer, bytes.Length);
                _pixelsDirty = true;
            };
            
            base.Initialize();

            _emulatorThread.Start();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            _running = false;
            _emulatorThread.Join();
            
            base.OnExiting(sender, args);
        }

        protected override void LoadContent()
        {
            BasicEffect = new BasicEffect(this.GraphicsDevice);
            GeneratedTexture = GenerateTexture2DWithTopLeftDiscoloration();
        }
        private Texture2D GenerateTexture2DWithTopLeftDiscoloration()
        {
            Texture2D t = new Texture2D(this.GraphicsDevice, 250, 250);
            var cdata = new Color[250 * 250];
            for (int i = 0; i < 250; i++)
            {
                for (int j = 0; j < 250; j++)
                {
                    if (i < 50 && j < 50)
                        cdata[i * 250 + j] = new Color(120, 120, 120, 250);
                    else
                        cdata[i * 250 + j] = Color.White;
                }
            }
            t.SetData(cdata);
            return t;
        }

        protected override void UnloadContent()
        {
            GeneratedTexture.Dispose();
        }

        protected override void Draw(GameTime gameTime)
        {
            SetCameraPosition2D(0, 0);
            
            if (!_pixelsDirty)
            {
                base.Draw(gameTime);
                return;
            }
            
            SetStates();
            SetUpBasicEffect();
            
            var cellW = (Viewport.Width / BUFFER_W) * 1;
            var cellH = (Viewport.Height / BUFFER_H) * 1;
            
            GraphicsDevice.Clear(Color.Black);

            for (var x = 0; x < BUFFER_H; x++)
            {
                for (var y = 0; y < BUFFER_H; y++)
                {
                    var b = _pixelBuffer[(y * BUFFER_H) + x];
                    if (b == 0x0) continue;
                    
                    var drawingRectangle = new Rectangle(x * cellW, y * cellH, cellW, cellH);
                    foreach (var pass in BasicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        DrawUserIndexedVertexRectangle(drawingRectangle);       
                    }
                }
            }
            
            base.Draw(gameTime);
            _pixelsDirty = false;
        }
        
        private void DebugPrint()
        {
            string ToHexN(byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
            string ToHexNN(ushort w) => $"0x{Convert.ToString(w, 16).PadLeft(4, '0').ToUpperInvariant()}";
            string ToBinaryN(byte b) => Convert.ToString(b, 2).PadLeft(8, '0')[..4];

            if (Registers.PC >= 0x00)//0x6B)
            {
                var spValue = "";
                try
                {
                    spValue = ToHexNN(Ram.GetNN(Registers.SP));
                    Clock.Cycle -= 8;
                }
                catch (IndexOutOfRangeException) {}
        
                Console.WriteLine(
                    $"{_lastInstructionName.PadRight(19)} - PC: {ToHexNN(Registers.PC)}, LY: {ToHexNN(Ram.LY)}, SP: {ToHexNN(Registers.SP)} ({spValue}), A: {ToHexN(Registers.A)}, B: {ToHexN(Registers.B)}, C: {ToHexN(Registers.C)}, D: {ToHexN(Registers.D)}, E: {ToHexN(Registers.E)}, H: {ToHexN(Registers.H)}, L: {ToHexN(Registers.L)}, F: {ToBinaryN(Registers.F)}");
            }
    
            if (_lastInstructionName.StartsWith("0x00E9") && !Registers.IsZero) throw new ApplicationException("Cart check failed. Exiting");
        }
        
        private void SetStates()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private void SetUpBasicEffect()
        {
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.TextureEnabled = true;
            BasicEffect.Texture = GeneratedTexture;

            // set up our matrix to match basic effect.
            Viewport viewport = GraphicsDevice.Viewport;
            //
            BasicEffect.World = Matrix.Identity;
            Vector3 cameraUp = Vector3.Transform(new Vector3(0, -1, 0), Matrix.CreateRotationZ(camera2DrotationZ));
            BasicEffect.View = Matrix.CreateLookAt(camera2DScrollPosition, camera2DScrollLookAt, cameraUp);
            // We could set up the world maxtrix this way and get the expected rotation but its not really proper.
            //basicEffect.World = Matrix.Identity * Matrix.CreateRotationZ(camera2DrotationZ);
            //basicEffect.View = Matrix.CreateLookAt(camera2DScrollPosition, camera2DScrollLookAt, new Vector3(0, -1, 0));
            BasicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
        }
        
        public void SetCameraPosition2D(int x, int y)
        {
            camera2DScrollPosition.X = x;
            camera2DScrollPosition.Y = y;
            camera2DScrollPosition.Z = -1;
            camera2DScrollLookAt.X = x;
            camera2DScrollLookAt.Y = y;
            camera2DScrollLookAt.Z = 0;
        }
        
        private void DrawUserIndexedVertexRectangle(Rectangle r)
        {
            var quad = new VertexPositionColorTexture[6];
            quad[0] = new VertexPositionColorTexture(new Vector3(r.Left, r.Top, 0f), Color.White, new Vector2(0f, 0f));
            quad[1] = new VertexPositionColorTexture(new Vector3(r.Left, r.Bottom, 0f), Color.White, new Vector2(0f, 1f));
            quad[2] = new VertexPositionColorTexture(new Vector3(r.Right, r.Bottom, 0f), Color.White, new Vector2(1f, 1f));
            quad[3] = new VertexPositionColorTexture(new Vector3(r.Right, r.Top, 0f), Color.White, new Vector2(1f, 0f));

            var indices = new short[6];
            if (GraphicsDevice.RasterizerState == RasterizerState.CullClockwise)
            {
                indices[0] = 0; indices[1] = 1; indices[2] = 2;
                indices[3] = 2; indices[4] = 3; indices[5] = 0;
            }
            else
            {
                indices[0] = 0; indices[1] = 2; indices[2] = 1;
                indices[3] = 2; indices[4] = 0; indices[5] = 3;
            }

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, quad, 0, 4, indices, 0, 2);
        }
    }
}