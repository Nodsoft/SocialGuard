﻿using SocialGuard.Common.Data.Models;

namespace SocialGuard.Web.Viewer;

public static class Utilities
{
	public static string GetTrustlistLevelBootstrapColor(this TrustlistUser user) => user.Entries?.Max(e => e.EscalationLevel) switch
	{
		>= 3 => "danger",
		2 => "warning",
		1 => "info",
		_ => "success"
	};

	public static string GetTrustlistLevelDisplayString(this TrustlistUser user) => user.Entries?.Max(e => e.EscalationLevel) switch
	{
		>= 3 => "Dangerous",
		2 => "Untrusted",
		1 => "Suspicious",
		_ => "Clean"
	};
}