using Microsoft.EntityFrameworkCore;
using MetaFlow.API.Data;
using MetaFlow.API.Repositories;
using MetaFlow.API.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5142");
builder.Environment.EnvironmentName = "Development";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var connectionString = builder.Configuration.GetConnectionString("OracleConnection") 
    ?? "User Id=METAFLOW_USER;Password=METAFLOW_PWD;Data Source=localhost:1521/XE;";

builder.Services.AddDbContext<MetaFlowDbContext>(options =>
    options.UseOracle(connectionString, oracleOptions =>
    {
        oracleOptions.MigrationsAssembly("MetaFlow.API");
        oracleOptions.CommandTimeout(300);
        oracleOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    })
    .AddInterceptors(new OracleCommandInterceptor()));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IMetaRepository, MetaRepository>();
builder.Services.AddScoped<IRegistroDiarioRepository, RegistroDiarioRepository>();
builder.Services.AddScoped<IResumoMensalRepository, ResumoMensalRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IRegistroDiarioService, RegistroDiarioService>();
builder.Services.AddScoped<IResumoMensalService, ResumoMensalService>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
builder.Services.AddScoped<IHealthService, HealthService>(); 
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<IEstatisticasService, EstatisticasService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MetaFlowDbContext>(
        name: "database",
        tags: ["database", "oracle"])
    .AddCheck("memory", 
        () => 
        {
            var memory = GC.GetTotalMemory(false) / 1024 / 1024;
            return memory < 500 
                ? HealthCheckResult.Healthy($"Memória OK: {memory}MB")
                : HealthCheckResult.Degraded($"Memória elevada: {memory}MB");
        },
        tags: new[] { "memory" })
    .AddCheck("api", 
        () => HealthCheckResult.Healthy("API respondendo"),
        tags: new[] { "api" })
    .AddCheck("ml-service",
        () => HealthCheckResult.Healthy("ML.NET Service operacional"),
        tags: new[] { "ml", "machine-learning" });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("x-api-version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "metaflow_chave_super_secreta_para_desenvolvimento_32_chars!";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MetaFlowAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MetaFlowClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Usuario", policy => policy.RequireRole("Usuario"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MetaFlow API v1",
        Version = "v1",
        Description = "**API Estável** - Versão principal com todos os endpoints básicos de acompanhamento de metas\n\n" +
                     "**Recursos:** Gestão de Metas, Registros Diários, Usuários\n" +
                     "**Status:** Production Ready",
        Contact = new OpenApiContact
        {
            Name = "Equipe MetaFlow",
            Url = new Uri("https://metaflow.com.br")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "MetaFlow API v2",
        Version = "v2", 
        Description = "**Versão Inteligente** - Endpoints aprimorados com ML.NET e análises preditivas\n\n" +
                     "**Novidades:**\n" +
                     "• Sistema de Recomendações com Machine Learning\n" +
                     "• Análise de Padrões de Comportamento\n" +
                     "• Previsão de Progresso de Metas\n" +
                     "• Dashboard Inteligente\n" +
                     "• Health checks avançados",
        Contact = new OpenApiContact
        {
            Name = "Equipe MetaFlow",
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando o esquema Bearer.\n\n" +
                     "Digite: **Bearer** {seu_token} \n\n" +
                     "Exemplo: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Unknown";
        var version = api.ActionDescriptor.EndpointMetadata
            .OfType<MapToApiVersionAttribute>()
            .FirstOrDefault()?.Versions.FirstOrDefault()?.ToString() ?? "v1";
        
        return new[] { $"{controllerName} ({version})" };
    });

 
c.OrderActionsBy(api =>
{
    var tagOrder = new Dictionary<string, int>
    {
        ["Autenticação"] = 1,
        ["Usuários"] = 2,
        ["Metas"] = 3,
        ["Registros Diários"] = 4,
        ["Resumos Mensais"] = 5,
        ["Dashboard"] = 6,
        ["Recomendações Inteligentes"] = 7,
        ["Análises"] = 8,
        ["Health Check"] = 9,
        ["API Info"] = 10,
        ["Home"] = 11
    };

    var methodOrder = new Dictionary<string, int>
    {
        ["POST"] = 1,    
        ["GET"] = 2,     
        ["PUT"] = 3,     
        ["PATCH"] = 4,   
        ["DELETE"] = 5   
    };

    var controllerName = api.ActionDescriptor.RouteValues["controller"];
    var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Auth"] = "Autenticação",
        ["Usuario"] = "Usuários",
        ["Meta"] = "Metas", 
        ["RegistroDiario"] = "Registros Diários",
        ["ResumoMensal"] = "Resumos Mensais",
        ["Dashboard"] = "Dashboard",
        ["Recomendacoes"] = "Recomendações Inteligentes",
        ["Analises"] = "Análises",
        ["Health"] = "Health Check"
    };

    var tag = tagMap.ContainsKey(controllerName!) ? tagMap[controllerName!] : "Outros";
    var method = api.HttpMethod ?? "";
    
    var tagOrderValue = tagOrder.ContainsKey(tag) ? tagOrder[tag] : 999;
    var methodOrderValue = methodOrder.ContainsKey(method) ? methodOrder[method] : 999;
    
    return $"{tagOrderValue:000}-{methodOrderValue:00}-{api.RelativePath}";
});

c.TagActionsBy(api =>
{
    var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Unknown";
    
    var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Auth"] = "Autenticação",
        ["Usuario"] = "Usuários",
        ["Meta"] = "Metas",
        ["RegistroDiario"] = "Registros Diários", 
        ["ResumoMensal"] = "Resumos Mensais",
        ["Dashboard"] = "Dashboard",
        ["Recomendacoes"] = "Recomendações Inteligentes",
        ["Analises"] = "Análises",
        ["Health"] = "Health Check"
    };

    var tag = tagMap.TryGetValue(controllerName!, out string? value) ? value : controllerName!;
    
    return new List<string> { tag };
});

c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

c.MapType<Enum>(() => new OpenApiSchema { Type = "string" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaFlow API v1 (Estável)");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "MetaFlow API v2 (Inteligente)");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "MetaFlow API Documentation";
        c.DisplayOperationId();
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        
        c.EnableTryItOutByDefault();
        c.DisplayRequestDuration();
    });
}

app.UseExceptionHandler("/error");
app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                       ?? Guid.NewGuid().ToString();
    
    context.Items["CorrelationId"] = correlationId;
    
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("""


<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>MetaFlow - Revolucionando o Futuro do Trabalho e Desenvolvimento Pessoal</title>
    <style>
        :root {
            --primary: #6366F1;
            --primary-dark: #4F46E5;
            --secondary: #10B981;
            --accent: #F59E0B;
            --dark: #1F2937;
            --darker: #111827;
            --light: #F9FAFB;
            --gray: #6B7280;
            --success: #059669;
            --gradient: linear-gradient(135deg, var(--primary) 0%, var(--secondary) 100%);
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body { 
            font-family: 'Inter', 'Segoe UI', system-ui, sans-serif; 
            background: linear-gradient(135deg, var(--darker) 0%, var(--dark) 100%);
            color: var(--light);
            min-height: 100vh;
            line-height: 1.6;
        }
        
        .container { 
            max-width: 1200px; 
            margin: 0 auto; 
            padding: 40px 20px;
        }
        
        /* HERO SECTION - Completely Redesigned */
        .hero {
            text-align: center;
            margin-bottom: 80px;
            padding: 100px 20px;
            background: 
                radial-gradient(circle at 20% 80%, rgba(99, 102, 241, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 80% 20%, rgba(16, 185, 129, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 40% 40%, rgba(245, 158, 11, 0.05) 0%, transparent 50%);
            border-radius: 32px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .hero-badge {
            display: inline-block;
            padding: 12px 24px;
            background: rgba(99, 102, 241, 0.2);
            border: 1px solid var(--primary);
            border-radius: 50px;
            color: var(--primary);
            font-weight: 600;
            margin-bottom: 30px;
            backdrop-filter: blur(10px);
        }
        
        .logo {
            font-size: 5em;
            font-weight: 900;
            background: var(--gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 20px;
            text-shadow: 0 0 80px rgba(99, 102, 241, 0.4);
            line-height: 1.1;
        }
        
        .tagline {
            font-size: 2.2em;
            color: var(--light);
            margin-bottom: 30px;
            font-weight: 300;
            line-height: 1.3;
            max-width: 800px;
            margin-left: auto;
            margin-right: auto;
        }
        
        .highlight {
            background: var(--gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            font-weight: 700;
        }
        
        .mission-statement {
            font-size: 1.3em;
            color: var(--gray);
            max-width: 700px;
            margin: 0 auto 50px;
            line-height: 1.7;
            background: rgba(255, 255, 255, 0.05);
            padding: 25px;
            border-radius: 20px;
            border-left: 4px solid var(--primary);
        }
        
        /* FUTURE OF WORK SECTION */
        .vision-section {
            background: rgba(255, 255, 255, 0.03);
            padding: 60px 40px;
            border-radius: 24px;
            margin-bottom: 60px;
            border: 1px solid rgba(255, 255, 255, 0.08);
        }
        
        .vision-title {
            text-align: center;
            font-size: 2.5em;
            margin-bottom: 50px;
            background: var(--gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        .future-pillars {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 30px;
            margin-bottom: 50px;
        }
        
        .pillar {
            background: rgba(255, 255, 255, 0.05);
            padding: 35px;
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
        }
        
        .pillar:hover {
            transform: translateY(-5px);
            border-color: var(--primary);
            box-shadow: 0 20px 40px rgba(99, 102, 241, 0.15);
        }
        
        .pillar-icon {
            font-size: 2.5em;
            margin-bottom: 20px;
        }
        
        .pillar h3 {
            color: var(--primary);
            margin-bottom: 15px;
            font-size: 1.4em;
        }
        
        /* SOLUÇÃO METAFLOW */
        .solution-section {
            margin-bottom: 80px;
        }
        
        .solution-header {
            text-align: center;
            margin-bottom: 50px;
        }
        
        .solution-title {
            font-size: 2.8em;
            margin-bottom: 20px;
            background: var(--gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        .solution-subtitle {
            font-size: 1.3em;
            color: var(--gray);
            max-width: 600px;
            margin: 0 auto;
        }
        
        .solution-features {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
            gap: 30px;
            margin-bottom: 50px;
        }
        
        .feature-card {
            background: rgba(255, 255, 255, 0.05);
            padding: 40px;
            border-radius: 20px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }
        
        .feature-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: var(--gradient);
        }
        
        .feature-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
        }
        
        .feature-card.ml-card::before {
            background: linear-gradient(135deg, var(--secondary) 0%, #059669 100%);
        }
        
        .feature-card.api-card::before {
            background: linear-gradient(135deg, var(--accent) 0%, #D97706 100%);
        }
        
        .feature-icon {
            font-size: 3em;
            margin-bottom: 20px;
            opacity: 0.9;
        }
        
        .feature-card h3 {
            font-size: 1.5em;
            margin-bottom: 15px;
            color: var(--light);
        }
        
        .feature-card p {
            color: var(--gray);
            margin-bottom: 20px;
            line-height: 1.6;
        }
        
        /* API ENDPOINTS */
        .endpoints-section {
            background: rgba(255, 255, 255, 0.03);
            padding: 50px;
            border-radius: 24px;
            margin-bottom: 60px;
        }
        
        .endpoints-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 20px;
            margin-top: 30px;
        }
        
        .endpoint-card {
            background: rgba(255, 255, 255, 0.05);
            padding: 25px;
            border-radius: 15px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            transition: all 0.3s ease;
        }
        
        .endpoint-card:hover {
            border-color: var(--primary);
            transform: translateY(-3px);
        }
        
        .endpoint-method {
            display: inline-block;
            padding: 6px 12px;
            background: var(--primary);
            color: white;
            border-radius: 6px;
            font-size: 0.8em;
            font-weight: 600;
            margin-bottom: 10px;
        }
        
        .endpoint-method.get { background: var(--secondary); }
        .endpoint-method.post { background: var(--accent); }
        .endpoint-method.put { background: #8B5CF6; }
        .endpoint-method.delete { background: #EF4444; }
        
        .endpoint-path {
            font-family: 'Monaco', 'Consolas', monospace;
            color: var(--light);
            margin-bottom: 10px;
            font-size: 0.9em;
        }
        
        .endpoint-desc {
            color: var(--gray);
            font-size: 0.85em;
        }
        
        /* BUTTONS & LINKS */
        .btn { 
            display: inline-flex;
            align-items: center;
            gap: 12px;
            padding: 18px 36px; 
            background: var(--gradient);
            color: white; 
            text-decoration: none; 
            border-radius: 15px; 
            margin: 8px;
            transition: all 0.3s ease;
            font-weight: 600;
            border: 2px solid transparent;
            font-size: 1em;
        }
        
        .btn:hover { 
            background: transparent;
            color: var(--primary);
            border-color: var(--primary);
            transform: translateY(-3px);
            box-shadow: 0 15px 35px rgba(99, 102, 241, 0.3);
        }
        
        .btn-outline {
            background: transparent;
            color: var(--primary);
            border: 2px solid var(--primary);
        }
        
        .btn-outline:hover {
            background: var(--primary);
            color: white;
        }
        
        .btn-success {
            background: linear-gradient(135deg, var(--secondary) 0%, var(--success) 100%);
        }
        
        .btn-success:hover {
            background: transparent;
            color: var(--secondary);
            border-color: var(--secondary);
        }
        
        .btn-accent {
            background: linear-gradient(135deg, var(--accent) 0%, #D97706 100%);
        }
        
        .btn-accent:hover {
            background: transparent;
            color: var(--accent);
            border-color: var(--accent);
        }
        
        .links { 
            text-align: center; 
            margin-top: 40px;
        }
        
        /* TECH STACK */
        .tech-stack {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            margin: 25px 0;
            justify-content: center;
        }
        
        .tech-item {
            background: rgba(99, 102, 241, 0.1);
            padding: 10px 18px;
            border-radius: 25px;
            border: 1px solid rgba(99, 102, 241, 0.3);
            font-size: 0.9em;
            color: var(--primary);
            transition: all 0.3s ease;
        }
        
        .tech-item:hover {
            background: rgba(99, 102, 241, 0.2);
            transform: scale(1.05);
        }
        
        .tech-item.ml { 
            background: rgba(16, 185, 129, 0.1);
            border-color: rgba(16, 185, 129, 0.3);
            color: var(--secondary);
        }
        
        .tech-item.api { 
            background: rgba(245, 158, 11, 0.1);
            border-color: rgba(245, 158, 11, 0.3);
            color: var(--accent);
        }
        
        /* FOOTER */
        .footer {
            text-align: center;
            margin-top: 80px;
            padding: 50px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            color: rgba(255, 255, 255, 0.7);
        }
        
        .footer-title {
            font-size: 1.5em;
            margin-bottom: 20px;
            background: var(--gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        @media (max-width: 768px) {
            .logo {
                font-size: 3em;
            }
            
            .tagline {
                font-size: 1.6em;
            }
            
            .hero {
                padding: 60px 20px;
            }
            
            .future-pillars,
            .solution-features {
                grid-template-columns: 1fr;
            }
            
            .vision-title {
                font-size: 2em;
            }
            
            .solution-title {
                font-size: 2.2em;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <!-- HERO SECTION - Completely Redesigned -->
        <div class="hero">
            <div class="hero-badge"> Futuro do Trabalho - Solução Inovadora</div>
            <div class="logo">MetaFlow</div>
            <div class="tagline">
                Reimaginando o <span class="highlight">Desenvolvimento Pessoal</span> 
                na Era da <span class="highlight">Inteligência Artificial</span>
            </div>
            
            <div class="mission-statement">
                <strong>Nossa Missão:</strong> Criar um futuro onde cada pessoa tenha ferramentas inteligentes 
                para alcançar seu potencial máximo, combinando tecnologia avançada com insights humanos 
                para transformar objetivos em conquistas reais.
            </div>
            
            <div class="links">
                <a href="/swagger" class="btn">
                    <span></span> Explorar Documentação
                </a>
                <a href="/api/v1/usuarios/registrar" class="btn btn-success">
                    <span></span> Começar Agora
                </a>
                <a href="/health" class="btn btn-outline">
                    <span></span> Health Check
                </a>
            </div>
        </div>

        <!-- FUTURO DO TRABALHO - VISÃO -->
        <div class="vision-section">
            <h2 class="vision-title"> O Futuro do Trabalho é Agora</h2>
            
            <div class="future-pillars">
                <div class="pillar">
                    <div class="pillar-icon"></div>
                    <h3>IA como Parceira Humana</h3>
                    <p>Machine Learning que complementa suas decisões, oferecendo insights personalizados sem substituir sua intuição e criatividade.</p>
                </div>
                
                <div class="pillar">
                    <div class="pillar-icon"></div>
                    <h3>Reskilling Contínuo</h3>
                    <p>Plataforma que identifica gaps de habilidades e sugere metas de aprendizado alinhadas com as demandas do mercado futuro.</p>
                </div>
                
                <div class="pillar">
                    <div class="pillar-icon"></div>
                    <h3>Bem-estar Integral</h3>
                    <p>Acompanhamento de saúde mental e produtividade para equilibrar desempenho profissional com qualidade de vida.</p>
                </div>
            </div>
            
            <div style="text-align: center;">
                <p style="color: var(--gray); font-size: 1.1em; max-width: 800px; margin: 0 auto;">
                    <strong>MetaFlow responde ao desafio</strong> criando uma ponte entre tecnologia avançada e desenvolvimento humano, 
                    onde a IA amplifica nossas capacidades e nos ajuda a navegar em um mundo de trabalho em constante transformação.
                </p>
            </div>
        </div>

        <!-- NOSSA SOLUÇÃO -->
        <div class="solution-section">
            <div class="solution-header">
                <h2 class="solution-title"> Nossa Solução Tecnológica</h2>
                <p class="solution-subtitle">
                    Plataforma completa que combina acompanhamento inteligente de metas com análise preditiva 
                    para revolucionar como as pessoas gerenciam seu desenvolvimento pessoal e profissional.
                </p>
            </div>
            
            <div class="solution-features">
                <!-- ML.NET Feature -->
                <div class="feature-card ml-card">
                    <div class="feature-icon"></div>
                    <h3>Sistema de Recomendações ML.NET</h3>
                    <p>Machine Learning que analisa seus padrões comportamentais e sugere metas personalizadas baseadas no seu perfil único.</p>
                    <div class="tech-stack">
                        <span class="tech-item ml">Classificação Multiclasse</span>
                        <span class="tech-item ml">Análise Preditiva</span>
                    </div>
                    <div style="margin-top: 20px;">
                        <a href="/swagger#/Recomendações%20Inteligentes/GerarRecomendacoes" class="btn btn-success" style="padding: 12px 24px; font-size: 0.9em;">
                            Testar ML.NET
                        </a>
                    </div>
                </div>
                
                <!-- API RESTful Feature -->
                <div class="feature-card api-card">
                    <div class="feature-icon"></div>
                    <h3>API RESTful Avançada</h3>
                    <p>Arquitetura moderna com versionamento, HATEOAS, paginação e documentação interativa completa.</p>
                    <div class="tech-stack">
                        <span class="tech-item api">.NET 9</span>
                        <span class="tech-item api">JWT Auth</span>
                        <span class="tech-item api">Swagger</span>
                    </div>
                    <div style="margin-top: 20px;">
                        <a href="/api" class="btn btn-accent" style="padding: 12px 24px; font-size: 0.9em;">
                            Explorar API
                        </a>
                    </div>
                </div>
                
                <!-- Monitoring Feature -->
                <div class="feature-card">
                    <div class="feature-icon"></div>
                    <h3>Monitoramento Completo</h3>
                    <p>Health checks, logging estruturado, tracing com Correlation ID e métricas em tempo real.</p>
                    <div class="tech-stack">
                        <span class="tech-item">Health Checks</span>
                        <span class="tech-item">Tracing</span>
                        <span class="tech-item">Logging</span>
                    </div>
                    <div style="margin-top: 20px;">
                        <a href="/health" class="btn btn-outline" style="padding: 12px 24px; font-size: 0.9em;">
                            Ver Saúde
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <!-- ENDPOINTS PRINCIPAIS -->
        <div class="endpoints-section">
            <h2 style="text-align: center; margin-bottom: 10px; color: var(--light);"> Endpoints Principais</h2>
            <p style="text-align: center; color: var(--gray); margin-bottom: 30px;">
                Acesse diretamente os recursos da plataforma através da API RESTful
            </p>
            
            <div class="endpoints-grid">
                <a href="/api/v1/Meta" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v1/Meta</div>
                    <div class="endpoint-desc">Listar todas as metas com paginação e HATEOAS</div>
                </a>
                
                <a href="/api/v1/Usuario" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v1/Usuario</div>
                    <div class="endpoint-desc">Gerenciar usuários e perfis</div>
                </a>
                
                <a href="/api/v1/RegistroDiario" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v1/RegistroDiario</div>
                    <div class="endpoint-desc">Check-ins diários de humor e produtividade</div>
                </a>
                
                <a href="/api/v1/ResumoMensal" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v1/ResumoMensal</div>
                    <div class="endpoint-desc">Relatórios mensais automatizados</div>
                </a>
                
                <a href="/api/v2/Recomendacoes" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v2/Recomendacoes</div>
                    <div class="endpoint-desc">Recomendações inteligentes com ML.NET</div>
                </a>
                
                <a href="/api/v2/Dashboard" class="endpoint-card">
                    <span class="endpoint-method get">GET</span>
                    <div class="endpoint-path">/api/v2/Dashboard</div>
                    <div class="endpoint-desc">Dashboard interativo com análises</div>
                </a>
            </div>
        </div>

        <!-- TECNOLOGIAS -->
        <div style="text-align: center; margin-bottom: 60px;">
            <h2 style="margin-bottom: 30px; color: var(--light);"> Stack Tecnológico</h2>
            <div class="tech-stack">
                <span class="tech-item">.NET 9</span>
                <span class="tech-item">Oracle Database</span>
                <span class="tech-item">Entity Framework</span>
                <span class="tech-item ml">ML.NET</span>
                <span class="tech-item">JWT Authentication</span>
                <span class="tech-item">RESTful API</span>
                <span class="tech-item">HATEOAS</span>
                <span class="tech-item">Swagger/OpenAPI</span>
                <span class="tech-item">Health Checks</span>
                <span class="tech-item">xUnit Tests</span>
            </div>
        </div>

        <!-- FOOTER -->
        <div class="footer">
            <div class="footer-title">MetaFlow Platform</div>
            <p><strong>Revolucionando o Futuro do Trabalho através do Desenvolvimento Pessoal Inteligente</strong></p>
            <p>Uma solução .NET completa que combina tecnologia avançada com insights humanos para criar oportunidades mais justas, inclusivas e sustentáveis.</p>
            
            <div class="links" style="margin-top: 30px;">
                <a href="/swagger" class="btn" style="padding: 12px 24px;">
                    Documentação
                </a>
                <a href="/api" class="btn btn-outline" style="padding: 12px 24px;">
                    API Info
                </a>
                <a href="/health" class="btn btn-accent" style="padding: 12px 24px;">
                    Health Check
                </a>
            </div>
        </div>
    </div>
</body>
</html>

""", "text/html")).WithTags("Home").WithName("HomePage");

app.MapGet("/index.html", () => Results.Redirect("/")).WithTags("Home");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            environment = app.Environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = $"{e.Value.Duration.TotalMilliseconds}ms",
                data = e.Value.Data
            }),
            totalDuration = $"{report.TotalDuration.TotalMilliseconds}ms"
        }, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await context.Response.WriteAsync(result);
    }
}).WithMetadata(new EndpointNameMetadata("health-check"));

app.MapGet("/api/info", () => 
{
    var assembly = Assembly.GetExecutingAssembly();
    return new
    {
        application = "MetaFlow API - Plataforma Inteligente de Acompanhamento de Metas Pessoais",
        version = assembly.GetName().Version?.ToString() ?? "1.0.0",
        environment = app.Environment.EnvironmentName,
        timestamp = DateTime.UtcNow,
        server_url = "http://localhost:5142",
        features = new[]
        {
            "Gestão de Metas Pessoais",
            "Registro Diário de Humor e Produtividade",
            "Dashboard de Progresso", 
            "Sistema de Recomendações com ML.NET",
            "Análise de Padrões Comportamentais",
            "Previsão de Progresso de Metas",
            "JWT Authentication",
            "Health Checks",
            "API Versioning",
            "HATEOAS Links",
            "Pagination"
        },
        links = new[]
        {
            new { rel = "documentation", href = "/swagger", method = "GET" },
            new { rel = "health", href = "/health", method = "GET" },
            new { rel = "api-info", href = "/api", method = "GET" },
            new { rel = "home", href = "/", method = "GET" }
        }
    };
}).WithTags("API Info").WithName("GetApiInfo");

app.MapGet("/api", () => new
{
    message = "Bem-vindo à MetaFlow API - Sua Plataforma Inteligente de Acompanhamento de Metas",
    description = "API RESTful desenvolvida em .NET 9 com Machine Learning para ajudar no desenvolvimento pessoal e profissional",
    server_url = "http://localhost:5142",
    documentation = "/swagger",
    health = "/health",
    home = "/",
    versions = new[] { 
        new { 
            version = "v1", 
            status = "stable",
            description = "API Estável - Funcionalidades básicas de gestão de metas",
            path = "/api/v1"
        },
        new { 
            version = "v2", 
            status = "intelligent", 
            description = "Versão Inteligente - Recursos avançados com ML.NET e análises preditivas",
            path = "/api/v2"
        }
    },
    core_features = new[] {
        "Definição e Acompanhamento de Metas",
        "Registro Diário de Humor e Produtividade", 
        "Dashboard com Métricas Visuais",
        "Sistema de Recomendações com Machine Learning",
        "Análise de Padrões Comportamentais",
        "Previsão de Progresso de Metas",
        "Relatórios Mensais Automatizados"
    },
    technology_stack = new[] {
        ".NET 9", "Oracle Database", "Entity Framework Core", 
        "ML.NET", "JWT Authentication", "Swagger/OpenAPI"
    },
    timestamp = DateTime.UtcNow,
    _links = new[]
    {
        new { rel = "self", href = "/api", method = "GET" },
        new { rel = "documentation", href = "/swagger", method = "GET" },
        new { rel = "health", href = "/health", method = "GET" },
        new { rel = "home", href = "/", method = "GET" },
        new { rel = "api-info", href = "/api/info", method = "GET" }
    }
}).WithTags("API Info");

app.MapControllers();

Console.WriteLine("==============================================");
Console.WriteLine("  METAFLOW API INICIADA COM SUCESSO!");
Console.WriteLine("==============================================");
Console.WriteLine($"  URL Principal: http://localhost:5142");
Console.WriteLine($"  Swagger Docs: http://localhost:5142/swagger");
Console.WriteLine($"   Health Check: http://localhost:5142/health");
Console.WriteLine($"  Página Inicial: http://localhost:5142");
Console.WriteLine($"  API Info: http://localhost:5142/api");
Console.WriteLine("==============================================");
Console.WriteLine(" COMEÇAR AGORA:");
Console.WriteLine("   1. Acesse: http://localhost:5142");
Console.WriteLine("   2. Clique em 'Registrar Agora'");
Console.WriteLine("   3. Explore a documentação no Swagger");
Console.WriteLine("==============================================");
Console.WriteLine(" RECURSOS DISPONÍVEIS:");
Console.WriteLine("   • Gestão de Metas Pessoais e Profissionais");
Console.WriteLine("   • Registro Diário de Humor e Produtividade");
Console.WriteLine("   • Dashboard com Análises Visuais");
Console.WriteLine("   • Sistema de Recomendações com ML.NET");
Console.WriteLine("   • Análise de Padrões Comportamentais");
Console.WriteLine("   • Previsão de Progresso de Metas");
Console.WriteLine("==============================================");
Console.WriteLine(" VERSÕES DA API:");
Console.WriteLine("   • v1 - API Estável (Funcionalidades Básicas)");
Console.WriteLine("   • v2 - Versão Inteligente (ML.NET + Análises)");
Console.WriteLine("==============================================");

await app.RunAsync();

public partial class Program { }