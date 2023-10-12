# proxy-tester

Checks network connectivity from current host to list of remote hosts via list of proxies with optional proxy credentials.

Special `noproxy` value in `proxies.csv` - use direct connection without a proxy, even proxy enabled in the _Network proxy settings_.

If you need to check connectivity using your default proxy, specify it's URI in `proxies.csv` explicitly.
