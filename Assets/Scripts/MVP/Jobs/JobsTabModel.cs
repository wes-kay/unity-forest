using System;
using System.Collections.Generic;
using Domain.MVP.Tab;

namespace Domain.MVP.Jobs
{
    /// <summary>
    /// Model for the Jobs tab. Manages job list and status state.
    /// </summary>
    public class JobsTabModel : TabModel
    {
        /// <summary>Currently selected job ID (empty = none).</summary>
        public string SelectedJobId { get; private set; }

        /// <summary>Fired when a job is selected.</summary>
        public event Action<string> OnJobSelected;

        /// <summary>Fired when job data refreshes.</summary>
        public event Action OnJobDataChanged;

        /// <summary>All jobs in the current category.</summary>
        public List<JobEntry> Jobs { get; private set; } = new List<JobEntry>();

        public JobsTabModel()
            : base("jobs", "Jobs", new[] { "available", "active", "completed" })
        {
        }

        public override void LoadFromService()
        {
            // TODO: Load job data from job system
        }

        public void SelectJob(string jobId)
        {
            SelectedJobId = jobId;
            OnJobSelected?.Invoke(jobId);
        }

        public void ClearSelection()
        {
            SelectedJobId = string.Empty;
        }

        /// <summary>Get jobs filtered by status category.</summary>
        public List<JobEntry> GetJobsByCategory(string categoryId)
        {
            switch (categoryId)
            {
                case "available":
                    return Jobs; // TODO: filter
                case "active":
                    return Jobs; // TODO: filter
                case "completed":
                    return Jobs; // TODO: filter
                default:
                    return Jobs;
            }
        }

        /// <summary>Add a job entry to the model.</summary>
        public void AddJob(string id, string title, string description, string status)
        {
            Jobs.Add(new JobEntry { id = id, title = title, description = description, status = status });
            OnJobDataChanged?.Invoke();
        }

        /// <summary>A simple job data struct.</summary>
        public struct JobEntry
        {
            public string id;
            public string title;
            public string description;
            public string status;
        }
    }
}
