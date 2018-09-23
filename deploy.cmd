@echo off
cd %~dp0
cp %DEPLOYMENT_SOURCE%\Atom %DEPLOYMENT_TARGET%\Atom
cp %DEPLOYMENT_SOURCE%\host.json %DEPLOYMENT_TARGET%\host.json
cp %DEPLOYMENT_SOURCE%\proxies.json %DEPLOYMENT_TARGET%\proxies.json
