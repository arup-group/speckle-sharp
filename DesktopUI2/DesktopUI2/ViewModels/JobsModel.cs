using ReactiveUI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace DesktopUI2.ViewModels
{
  public class JobModel
  {
    public string JobCode { get; set; }
    public string JobNameShort { get; set; }
    public string JobNameLong { get; set; }
    public string RegionCode { get; set; }

    public string JobDisplayName { get { return $"{JobCode} {JobNameLong}"; } }
    public ProjectModel Project { get; set; }

    public JobModel() { }
  }

  public class ProjectModel
  {
    public string ScopeOfService { get; set; }
    public string ScopeOfWorks { get; set; }
    public string ProjectDirectorName { get; set; }
    public string ProjectDirectorEmail { get; set; }
    public string ProjectManagerName { get; set; }
    public string ProjectManagerEmail { get; set; }
    public string ProjectUrl { get; set; }

    public ProjectModel() { }
  }

  public class JobSearchOptions
  {
    public List<JobModel> jobs { get; set; }
    public JobSearchOptions() { }

  }
}
