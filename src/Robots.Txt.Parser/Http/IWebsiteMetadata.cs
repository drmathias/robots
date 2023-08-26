using System;

namespace Robots.Txt.Parser.Http;

public interface IWebsiteMetadata
{
    static abstract Uri BaseAddress { get; }
}
