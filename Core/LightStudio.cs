using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;

namespace Fusee.LightStudio.Core
{

    class Renderer : SceneVisitor
    {
        public RenderContext RC;
        public IShaderParam AlbedoParam;
        public IShaderParam ShininessParam;
        private IShaderParam TextureParam;
        private IShaderParam Texture2Param;
        private IShaderParam TexMixParam;
        private ITexture _maleModelTexture;
        private ITexture _maleModelTextureNM;

        public IShaderParam LightDirParam;

        public float4x4 View;
        private Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();
        private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();
        private Mesh LookupMesh(MeshComponent mc)
        {
            Mesh mesh;
            if (!_meshes.TryGetValue(mc, out mesh))
            {
                mesh = new Mesh
                {
                    Vertices = mc.Vertices,
                    Normals = mc.Normals,
                    UVs = mc.UVs,
                    Triangles = mc.Triangles

                };
                _meshes[mc] = mesh;
            }
            return mesh;
        }

        public Renderer(RenderContext rc)
        {
            RC = rc;
            // Initialize the shader(s)

            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);
            AlbedoParam = RC.GetShaderParam(shader, "albedo");
            ShininessParam = RC.GetShaderParam(shader, "shininess");
            ImageData maleModelED = AssetStorage.Get<ImageData>("maleModel_ED.jpg");
            _maleModelTexture = RC.CreateTexture(maleModelED);
            ImageData maleModelNM = AssetStorage.Get<ImageData>("maleFigure_NM_switch_red_ch.png");
            _maleModelTextureNM = RC.CreateTexture(maleModelNM);
            TextureParam = RC.GetShaderParam(shader, "texture");
            Texture2Param = RC.GetShaderParam(shader, "normalTex");
            TexMixParam = RC.GetShaderParam(shader, "texmix");
            LightDirParam = RC.GetShaderParam(shader, "lightdir");


        }

        protected override void InitState()
        {
            _model.Clear();
            _model.Tos = float4x4.Identity;
        }
        protected override void PushState()
        {
            _model.Push();
        }
        protected override void PopState()
        {
            _model.Pop();
            RC.ModelView = View * _model.Tos;
        }
        [VisitMethod]
        void OnMesh(MeshComponent mesh)
        {
            RC.Render(LookupMesh(mesh));
        }
        [VisitMethod]
        void OnMaterial(MaterialComponent material)
        {
            // LightDir in ModelKoordinate angeben, im Moment sind es ViewKoordinaten
            // falls Richtungsvektor, dann Mult. mit RC.InvTransModelView.
            // falls Punktvektor, dann mit RC.ModelView


            
            //RC.SetShaderParam(LightDirParam, new float3(0, 0, -1));
            RC.SetShaderParamTexture(TextureParam, _maleModelTexture);
            RC.SetShaderParam(TexMixParam, 1.0f);
            
           
        }
        [VisitMethod]
        void OnTransform(TransformComponent xform)
        {
            _model.Tos *= xform.Matrix();
            RC.ModelView = View * _model.Tos;
        }
    }


    [FuseeApplication(Name = "Light Studio", Description = "A virtual light studio.")]
    public class LightStudio : RenderCanvas
    {
        private Mesh _mesh;
        private TransformComponent _wheelBigL;

        private IShaderParam _albedoParam;
        private float _alpha = 0.001f;
        private float _beta;
        
        private SceneOb _root;
        private SceneContainer _maleModel;
        private SceneContainer _sphere;

        //now implement the float 3x3 for the lightdir
        private float _lightDir;

        private Renderer _renderer;

        // Init is called on startup. 
        public override void Init()
        {
            // Load some meshes
            _maleModel = AssetStorage.Get<SceneContainer>("Model.fus");
           
            
            _renderer = new Renderer(RC);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0, 0, 0, 1);

            
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Viewports setzen

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }


            // Eingabe abholen, und durchreichen float3 an shader und die Manipulation der Lichtposition mittels Keyboard
            
            if (Keyboard.GetKey(KeyCodes.Left))
            {
                _lightDir = _lightDir - 2f;
            }
            else if (Keyboard.GetKey(KeyCodes.Right))
            {
                _lightDir += 2f;
            }


            // Viewports setzen

            RC.Viewport(0, 0, Width, Height);

            // Setup matrices
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 100);
            float4x4 view = float4x4.CreateTranslation(0, -52, 30) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta) * float4x4.CreateTranslation(0, 0, 0);



            _renderer.View = view;
            _renderer.RC.SetShaderParam(_renderer.LightDirParam, new float3(_lightDir, 0, 0) * RC.InvTransModelView);
            _renderer.Traverse(_maleModel.Children);

            // Hier kleiner Viewport setzen geänderte Proj und View Matrizen setzen
            // nochmal rendern
            // render View und Viewport setzen
            RC.Projection = float4x4.CreateOrthographic(52, 30, 0, 70);
            _renderer.View = float4x4.CreateRotationX(-3.141592f / 2) * float4x4.CreateTranslation(0, -100, 0);

            RC.Viewport(0, Height - 400, 500, 400);
            _renderer.Traverse(_maleModel.Children);


            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 2000);
            RC.Projection = projection;
        }

    }
}