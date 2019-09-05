内容安全策略（CSP）是一个增加的安全层，可帮助检测和缓解某些类型的攻击，包括跨站点脚本（XSS）和数据注入攻击。这些攻击用于从数据窃取到站点破坏或恶意软件分发的所有内容（深入CSP）

简而言之，CSP是网页控制允许加载哪些资源的一种方式。例如，页面可以显式声明允许从中加载JavaScript，CSS和图像资源。这有助于防止跨站点脚本（XSS）攻击等问题。

它也可用于限制协议，例如限制通过HTTPS加载的内容。CSP通过 Content-Security-Policy HTTP响应中的标头实现。

启用CSP,您需要配置Web服务器以返回Content-Security-PolicyHTTP标头。那么在这篇文章中，我们将要尝试将CSP添加到ASP.NET Core应用程序中。

1
2
3
4
5
6
app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("Content-Security-Policy",
                            "default-src 'self'; report-uri /cspreport");
                await next();
            });
　　在Home/Index中引入cdn文件，然后我们启动项目，看看会发生什么！



运行并观察错误。加载页面时，浏览器拒绝从远程源加载。

 

 所以我们可以组织CSP来控制我们的白名单，在配置当中需要填写来源以及内容，以下是常用限制的选项。

来源：

复制代码
*: 允许任何网址。
‘self’: 允许所提供页面的来源。请注意，单引号是必需的。
‘none’: 不允许任何来源。请注意，单引号是必需的。
Host: 允许指定的互联网主机（按名称或IP地址）。通配符（星号字符）可用于包括所有子域，例如http：//*.foo.com
‘unsafe-line’: 允许内联脚本
‘nonce-[base64-value]’: 允许具有特定nonce的内联脚本（使用一次的数字）。对于每个HTTP请求/响应，应该对nonce进行加密和唯一。
复制代码
 指令：

复制代码
script-src：定义有效的JavaScript源
style-src：定义样式表的有效来源
img-src：定义有效的图像源
connect-src：定义可以进行AJAX调用的有效源
font-src：定义有效的字体来源
object-src：定义<object>，<embed>和<applet>元素的有效源
media-src：定义有效的音频和视频源
form-action：定义可用作HTML <form>操作的有效源。
default-src：指定加载内容的默认策略
复制代码
我们可以在可重用的中间件中封装构建和添加CSP头。以下是一个让您入门的示例。你可以根据需要扩展它。首先，创建一个用于保存源的类。

复制代码
public class CspOptions
    {
        public List<string> Defaults { get; set; } = new List<string>();
        public List<string> Scripts { get; set; } = new List<string>();
        public List<string> Styles { get; set; } = new List<string>();
        public List<string> Images { get; set; } = new List<string>();
        public List<string> Fonts { get; set; } = new List<string>();
        public List<string> Media { get; set; } = new List<string>();
    }
复制代码
 开发一个中间件一定是需要一个构造器的，这将用于.net core 的注入到运行环境中。

复制代码
public sealed class CspOptionsBuilder  
 {  
     private readonly CspOptions options = new CspOptions();  
       
     internal CspOptionsBuilder() { }  
  
     public CspDirectiveBuilder Defaults { get; set; } = new CspDirectiveBuilder();  
     public CspDirectiveBuilder Scripts { get; set; } = new CspDirectiveBuilder();  
     public CspDirectiveBuilder Styles { get; set; } = new CspDirectiveBuilder();  
     public CspDirectiveBuilder Images { get; set; } = new CspDirectiveBuilder();  
     public CspDirectiveBuilder Fonts { get; set; } = new CspDirectiveBuilder();  
     public CspDirectiveBuilder Media { get; set; } = new CspDirectiveBuilder();  
  
     internal CspOptions Build()  
     {  
         this.options.Defaults = this.Defaults.Sources;  
         this.options.Scripts = this.Scripts.Sources;  
         this.options.Styles = this.Styles.Sources;  
         this.options.Images = this.Images.Sources;  
         this.options.Fonts = this.Fonts.Sources;  
         this.options.Media = this.Media.Sources;  
         return this.options;  
     }  
 }  
  
 public sealed class CspDirectiveBuilder  
 {  
     internal CspDirectiveBuilder() { }  
  
     internal List<string> Sources { get; set; } = new List<string>();  
  
     public CspDirectiveBuilder AllowSelf() => Allow("'self'");  
     public CspDirectiveBuilder AllowNone() => Allow("none");  
     public CspDirectiveBuilder AllowAny() => Allow("*");  
  
     public CspDirectiveBuilder Allow(string source)  
     {  
         this.Sources.Add(source);  
         return this;  
     }  
 }  
复制代码
 好了，我们创建一个中间件。

复制代码
namespace XSSDefenses.XSSDefenses.MiddlerWare
{
    public sealed class CspOptionMiddlerWare
    {
        private const string HEADER = "Content-Security-Policy";
        private readonly RequestDelegate next;
        private readonly CspOptions options;

        public CspOptionMiddlerWare(
            RequestDelegate next, CspOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add(HEADER, GetHeaderValue());
            await this.next(context);
        }

        private string GetHeaderValue()
        {
            var value = "";
            value += GetDirective("default-src", this.options.Defaults);
            value += GetDirective("script-src", this.options.Scripts);
            value += GetDirective("style-src", this.options.Styles);
            value += GetDirective("img-src", this.options.Images);
            value += GetDirective("font-src", this.options.Fonts);
            value += GetDirective("media-src", this.options.Media);
            return value;
        }
        private string GetDirective(string directive, List<string> sources)
            => sources.Count > 0 ? $"{directive} {string.Join(" ", sources)}; " : "";
    }
}
复制代码
 以及设置它的扩展方法。


namespace XSSDefenses.XSSDefenses.Extensions
{
    public static class CspMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsp(
             this IApplicationBuilder app, Action<CspOptionsBuilder> builder)
        {
            var newBuilder = new CspOptionsBuilder();
            builder(newBuilder);
 
            var options = newBuilder.Build();
            return app.UseMiddleware<CspOptionMiddlerWare>(options);
        }
    }
}
我们现在可以在Startup类中配置中间件。

app.UseCsp(builder =>
            {
                builder.Styles.AllowSelf()
                .Allow(@"https://ajax.aspnetcdn.com/");
            });
 启动发现，观察网络资源。浏览器已经允许本地和远程资源。

