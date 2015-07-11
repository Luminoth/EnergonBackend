# TODO

1. TLS for auth/chat connections
2. Test account generator script (username/password input, SQL output)
  * Python script, make sure MD5/SHA512 bits work correctly
3. "New Account" page in Launcher
 * Remove dev account from post-deploy script
5. ASP.NET account setup page?
6. Self-Host MVC instead of implementing an HttpListener
 * Requires ASP.NET 5 and MVC 6 which is coming with Visual Studio 2015
 * http://www.asp.net/vnext/overview/aspnet-vnext/create-a-web-api-with-mvc-6 (Microsoft.AspNet.Hosting ?)
7. All of the *Test projects need to be renamed (and moved on the filesystem!) to *.Test to better match their namespaces
 * Probably easiest to just delete and remake them