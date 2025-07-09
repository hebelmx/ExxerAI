using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExxerAI.Orchestration.Configuration
{
    public class SharedConfig
    {
        public DatabaseConfiguration DatabaseConfig { get; set; } = new DatabaseConfiguration();
        public LocalAIConfiguration LocalAIConfig { get; set; } = new LocalAIConfiguration();
        public SearchConfiguration SearchConfig { get; set; } = new SearchConfiguration();
        public QdrantConfiguration QdrantConfig { get; set; } = new QdrantConfiguration();
        public RedisConfiguration RedisConfig { get; set; } = new RedisConfiguration();
        public VectorDatabaseConfiguration VectorDatabaseConfig { get; set; } = new VectorDatabaseConfiguration();
        public MonitoringConfiguration MonitoringConfig { get; set; } = new MonitoringConfiguration();
        public NetworkConfiguration NetworkConfig { get; set; } = new NetworkConfiguration();
        public SecurityConfiguration SecurityConfig { get; set; } = new SecurityConfiguration();
        public PrometheusConfiguration PrometheusConfig { get; set; } = new PrometheusConfiguration();
        public LocalAIStackConfiguration LocalAIStackConfig { get; set; } = new LocalAIStackConfiguration();
    }
}