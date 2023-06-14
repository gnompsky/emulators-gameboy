using GameBoyEmulator.Core;
using GameBoyEmulator.Core.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private const int BUFFER_W = 160;
        private const int BUFFER_H = 144;
        private DynamicSoundEffectInstance _sound;
        private string _lastInstructionName = "";
        private bool _running = true;

        private Thread _emulatorThread = new Thread(() =>
        {
            while (Instance._running)
            {
                GameBoy.Instance.Step();
                Thread.Sleep(100);
                //DebugPrint();
            }
        });

        public Game()
        {
            Instance = this;
            
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = BUFFER_W * 5;
            _graphics.PreferredBackBufferHeight = BUFFER_H * 5;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            GameBoy.Instance.Reset();

            var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/cpu_instrs.gb");
            //var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/Tetris (World) (Rev 1).gb");
            GameBoy.Instance.LoadRom(romBytes);

            _sound = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);
            
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

        protected override void Update(GameTime gameTime)
        {
            // TODO: Get audio samples and play
            // var buffer = Ram.HardwareRegisters.Audio.AudioStream.ToArray();
            // if (buffer.Length > 0)
            // {
            //     _sound.SubmitBuffer(buffer);
            //     _sound.Play();
            // }

            base.Update(gameTime);
        }

        private int curX = 0;
        private int curY = 0;

        private bool done = false;
        protected override void Draw(GameTime gameTime)
        {
            SetCameraPosition2D(0, 0);

            SetStates();
            SetUpBasicEffect();
            
            var cellW = (Viewport.Width / BUFFER_W) * 1;
            var cellH = (Viewport.Height / BUFFER_H) * 1;
            
            while (GameBoy.Instance.Pixels.TryDequeue(out var pixel))
            {
                if (!done)
                {
                    var drawingRectangle = new Rectangle(curX * cellW, curY * cellH, cellW, cellH);
                    foreach (var pass in BasicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        DrawUserIndexedVertexRectangle(drawingRectangle, pixel.Color);
                    }

                    //done = true;
                }

                Console.Write("x");
                curX++;
                if (curX >= BUFFER_W)
                {
                    curX = 0;
                    curY++;
                    Console.WriteLine();

                    if (curY >= BUFFER_H)
                    {
                        curY = 0;
                    }
                }
            }

            base.Draw(gameTime);
        }
        
        // private void DebugPrint()
        // {
        //     string ToHexN(byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
        //     string ToHexNN(ushort w) => $"0x{Convert.ToString(w, 16).PadLeft(4, '0').ToUpperInvariant()}";
        //     string ToBinaryN(byte b) => Convert.ToString(b, 2).PadLeft(8, '0')[..4];
        //
        //     if (Registers.PC >= 0x00)//0x6B)
        //     {
        //         var spValue = "";
        //         try
        //         {
        //             spValue = ToHexNN(Ram.GetNN(Registers.SP));
        //             Clock.Cycle -= 8;
        //         }
        //         catch (IndexOutOfRangeException) {}
        //
        //         Console.WriteLine(
        //             $"{_lastInstructionName.PadRight(19)} - PC: {ToHexNN(Registers.PC)}, LY: {ToHexNN(Ram.LY)}, SP: {ToHexNN(Registers.SP)} ({spValue}), A: {ToHexN(Registers.A)}, B: {ToHexN(Registers.B)}, C: {ToHexN(Registers.C)}, D: {ToHexN(Registers.D)}, E: {ToHexN(Registers.E)}, H: {ToHexN(Registers.H)}, L: {ToHexN(Registers.L)}, F: {ToBinaryN(Registers.F)}");
        //     }
        //
        //     if (_lastInstructionName.StartsWith("0x00E9") && !Registers.IsZero) throw new ApplicationException("Cart check failed. Exiting");
        // }
        
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
        
        private void DrawUserIndexedVertexRectangle(Rectangle r, Colors color)
        {
            Color c = color switch
            {
                Colors.White => new Color(155, 188, 15),
                Colors.LightGrey => new Color(139, 172, 15),
                Colors.DarkGrey => new Color(48, 98, 48),
                Colors.Black => new Color(15, 56, 16),
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };

            var quad = new VertexPositionColorTexture[6];
            quad[0] = new VertexPositionColorTexture(new Vector3(r.Left, r.Top, 0f), c, new Vector2(0f, 0f));
            quad[1] = new VertexPositionColorTexture(new Vector3(r.Left, r.Bottom, 0f), c, new Vector2(0f, 1f));
            quad[2] = new VertexPositionColorTexture(new Vector3(r.Right, r.Bottom, 0f), c, new Vector2(1f, 1f));
            quad[3] = new VertexPositionColorTexture(new Vector3(r.Right, r.Top, 0f), c, new Vector2(1f, 0f));

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