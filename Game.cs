using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

class Game : GameWindow
{
    private int _vao;
    private int _vbo;
    private int _shaderProgram;
    private int _uniformModel;
    private int _uniformProjection;
    private float _rotation;

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        float[] vertices = {
             0.0f,  0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
             0.5f, -0.5f, 0.0f
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        string vertexShaderSrc = @"
        #version 330 core
        layout(location = 0) in vec3 aPosition;
        uniform mat4 uModel;
        uniform mat4 uProjection;
        void main()
        {
            gl_Position = uProjection * uModel * vec4(aPosition, 1.0);
        }";

        string fragmentShaderSrc = @"
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(1.0, 0.5, 0.2, 1.0);
        }";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSrc);
        GL.CompileShader(vertexShader);
        CheckShader(vertexShader, "VERTEX");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSrc);
        GL.CompileShader(fragmentShader);
        CheckShader(fragmentShader, "FRAGMENT");

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        _uniformModel = GL.GetUniformLocation(_shaderProgram, "uModel");
        _uniformProjection = GL.GetUniformLocation(_shaderProgram, "uProjection");

        if (_uniformModel == -1 || _uniformProjection == -1)
            Console.WriteLine("❌ Uniform location error.");
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _rotation += 1f * (float)args.Time;

        Matrix4 model = Matrix4.CreateRotationZ(_rotation);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(_shaderProgram);

        GL.UniformMatrix4(_uniformModel, false, ref model);
        GL.UniformMatrix4(_uniformProjection, false, ref projection);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        SwapBuffers();
    }

    private void CheckShader(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"❌ Error compiling {type} shader:\n{log}");
            throw new Exception(log);
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);
    }
}