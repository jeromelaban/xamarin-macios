#load "utils.csx"

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Xamarin.Provisioning;
using Xamarin.Provisioning.Model;

// Provision Mono, Mono, Objective-Sharpie, provisioning profiles.
//
// We get Mono from the current commit's MIN_MONO_URL value in Make.config
//
// Overrides:
// * Each download URL can be overriden by setting an environment variable (MIN_MONO_URL).
// * The current commit can be overridden by setting the PROVISION_FROM_COMMIT variable. This is usually easier than overriding each url.

var commit = Environment.GetEnvironmentVariable ("BUILD_SOURCEVERSION");
var provision_from_commit = Environment.GetEnvironmentVariable ("PROVISION_FROM_COMMIT") ?? commit;

string FindVariable (string variable, bool throwIfNotFound = true)
{
	var value = FindConfigurationVariable (variable, provision_from_commit);
	if (!string.IsNullOrEmpty (value))
		return value;

	if (!string.IsNullOrEmpty (value))
		return value;

	if (!throwIfNotFound)
		return null;

	throw new Exception ($"Could not find {variable} in environment nor in the commit's ({commit}) manifest.");
}

if (string.IsNullOrEmpty (provision_from_commit)) {
	Console.Error.WriteLine ($"Either BUILD_SOURCEVERSION or PROVISION_FROM_COMMIT must be set.");
	Environment.Exit (1);
	return 1;
}
Console.WriteLine ($"Provisioning from {provision_from_commit}...");

InstallPackage ("Mono", FindVariable ("MIN_MONO_URL"));
InstallPackage ("Objective-Sharpie", FindVariable ("MIN_SHARPIE_URL"));

// Provisioning profiles
Console.WriteLine ("Provisioning provisioning profiles...");
Exec ($"../../../maccore/tools/install-qa-provisioning-profiles.sh");

// .NET core
// The version number here must match the one in Xamarin.Tests.Configuration:CreateGlobalConfig (tests/sampletester/Configuration.cs).
DotNetCoreSdk ("2.2.204");
