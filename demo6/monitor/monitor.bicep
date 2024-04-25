// https://ms.portal.azure.com/#view/HubsExtension/DeploymentDetailsBlade/~/overview/id/%2Fsubscriptions%2Fb47beaaf-7461-4b34-844a-7105d6b8c0d7%2FresourceGroups%2Fhunter-test1-rg%2Fproviders%2FMicrosoft.Resources%2Fdeployments%2FCIArmTemplateOnboardingDeployment-d760e29a-ebee-4c1a-8518-667bd3


param project_name string = 'hunter'
param environment string = 'demo6'

param location string = resourceGroup().location
param aks_name string = toLower('${project_name}-${environment}-aks')



param log_analytics_workspace_name string = toLower('${project_name}-${environment}-log-analytics-workspace')
param log_dcr_name string = toLower('${project_name}-${environment}-log-dcr')
param log_dcra_name string = toLower('${project_name}-${environment}-log-dcra')

param montior_workspace_name string = toLower('${project_name}-${environment}-monitor-workspace')
param monitor_dce_name string = toLower('${project_name}-${environment}-monitor-dce')
param monitor_dcr_name string = toLower('${project_name}-${environment}-monitor-dcr')
param monitor_dcra_name string = toLower('${project_name}-${environment}-monitor-dcra')

param prometheus_node_recording_rules_name string = toLower('${project_name}-${environment}-prometheus-node-recording-rules')
param prometheus_kubernetes_recording_rules_name string = toLower('${project_name}-${environment}-prometheus-kubernetes-recording-rules')

param grafana_name string = toLower('${project_name}-${environment}-grafana')


resource aks_resource 'Microsoft.ContainerService/managedClusters@2024-01-01' = {
  name: aks_name
  location: location
  properties: {
    addonProfiles: {
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: log_analytics_workspace_resource.id
          useAADAuth: 'true'
        }
      }
    }
    azureMonitorProfile: {
      metrics: {
        enabled: true
      }
    }
  }
}

resource log_analytics_workspace_resource 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: log_analytics_workspace_name
  location: location
}

resource log_dcr_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: log_dcr_name
  location: location
  kind: 'Linux'
  properties: {
    dataSources: {
      extensions: [
        {
          name: 'ContainerInsightsExtension'
          extensionName: 'ContainerInsights'
          streams: [
            'Microsoft-ContainerLog'
            'Microsoft-ContainerLogV2'
            'Microsoft-KubeEvents'
            'Microsoft-KubePodInventory'
          ]
          extensionSettings: {
            dataCollectionSettings: {
              interval: '1m'
              namespaceFilteringMode: 'Off'
              enableContainerLogV2: true
            }
          }
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-ContainerLog'
          'Microsoft-ContainerLogV2'
          'Microsoft-KubeEvents'
          'Microsoft-KubePodInventory'
        ]
        destinations: ['ciworkspace']
      }
    ]
    destinations: {
      logAnalytics: [
        {
          name: 'ciworkspace'
          workspaceResourceId: log_analytics_workspace_resource.id
        }
      ]
    }
  }
}

resource log_dcra_resource 'Microsoft.Insights/dataCollectionRuleAssociations@2022-06-01' = {
  name: log_dcra_name
  scope: aks_resource
  properties: {
    description: 'Association of data collection rule. Deleting this association will break the data collection for this AKS Cluster.'
    dataCollectionRuleId: log_dcr_resource.id
  }
}

resource monitor_workspace_resource 'Microsoft.Monitor/accounts@2023-04-03' = {
  name: montior_workspace_name
  location: location
}

resource monitor_dce_resource 'Microsoft.Insights/dataCollectionEndpoints@2022-06-01' = {
  name: monitor_dce_name
  location: location
  kind: 'Linux'
  properties: {}
}

resource monitor_dcr_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: monitor_dcr_name
  location: location
  kind: 'Linux'
  properties: {
    dataCollectionEndpointId: monitor_dce_resource.id
    dataSources: {
      prometheusForwarder: [
        {
          name: 'PrometheusDataSource'
          streams: ['Microsoft-PrometheusMetrics']
        }
      ]
    }
    dataFlows: [
      {
        streams: ['Microsoft-PrometheusMetrics']
        destinations: ['MonitoringAccount1']
      }
    ]
    destinations: {
      monitoringAccounts: [
        {
          name: 'MonitoringAccount1'
          accountResourceId: monitor_workspace_resource.id
        }
      ]
    }
  }
}

resource monitor_dcra_resource 'Microsoft.Insights/dataCollectionRuleAssociations@2022-06-01' = {
  name: monitor_dcra_name
  scope: aks_resource
  properties: {
    description: 'Association of data collection rule. Deleting this association will break the prometheus metrics data collection for this AKS Cluster.'
    dataCollectionRuleId: monitor_dcr_resource.id
  }
}

resource prometheus_node_recoding_rules_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheus_node_recording_rules_name
  location: location
  properties: {
    enabled: true
    description: 'Node Recording Rules RuleGroup'
    scopes: [monitor_workspace_resource.id, aks_resource.id]
    interval: 'PT1M'
    rules: [
      {
        record: 'instance:node_num_cpu:sum'
        expression: 'count without (cpu, mode) (  node_cpu_seconds_total{job="node",mode="idle"})'
      }
      {
        record: 'instance:node_cpu_utilisation:rate5m'
        expression: '1 - avg without (cpu) (  sum without (mode) (rate(node_cpu_seconds_total{job="node", mode=~"idle|iowait|steal"}[5m])))'
      }
      {
        record: 'instance:node_load1_per_cpu:ratio'
        expression: '(  node_load1{job="node"}/  instance:node_num_cpu:sum{job="node"})'
      }
      {
        record: 'instance:node_memory_utilisation:ratio'
        expression: '1 - (  (    node_memory_MemAvailable_bytes{job="node"}    or    (      node_memory_Buffers_bytes{job="node"}      +      node_memory_Cached_bytes{job="node"}      +      node_memory_MemFree_bytes{job="node"}      +      node_memory_Slab_bytes{job="node"}    )  )/  node_memory_MemTotal_bytes{job="node"})'
      }
      {
        record: 'instance:node_vmstat_pgmajfault:rate5m'
        expression: 'rate(node_vmstat_pgmajfault{job="node"}[5m])'
      }
      {
        record: 'instance_device:node_disk_io_time_seconds:rate5m'
        expression: 'rate(node_disk_io_time_seconds_total{job="node", device!=""}[5m])'
      }
      {
        record: 'instance_device:node_disk_io_time_weighted_seconds:rate5m'
        expression: 'rate(node_disk_io_time_weighted_seconds_total{job="node", device!=""}[5m])'
      }
      {
        record: 'instance:node_network_receive_bytes_excluding_lo:rate5m'
        expression: 'sum without (device) (  rate(node_network_receive_bytes_total{job="node", device!="lo"}[5m]))'
      }
      {
        record: 'instance:node_network_transmit_bytes_excluding_lo:rate5m'
        expression: 'sum without (device) (  rate(node_network_transmit_bytes_total{job="node", device!="lo"}[5m]))'
      }
      {
        record: 'instance:node_network_receive_drop_excluding_lo:rate5m'
        expression: 'sum without (device) (  rate(node_network_receive_drop_total{job="node", device!="lo"}[5m]))'
      }
      {
        record: 'instance:node_network_transmit_drop_excluding_lo:rate5m'
        expression: 'sum without (device) (  rate(node_network_transmit_drop_total{job="node", device!="lo"}[5m]))'
      }
    ]
  }
}

resource prometheus_kubernetes_recoding_rules_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheus_kubernetes_recording_rules_name
  location: location
  properties: {
    enabled: true
    description: 'Kubernetes Recording Rules RuleGroup'
    scopes: [monitor_workspace_resource.id, aks_resource.id]
    interval: 'PT1M'
    rules: [
      {
        record: 'node_namespace_pod_container:container_cpu_usage_seconds_total:sum_irate'
        expression: 'sum by (cluster, namespace, pod, container) (  irate(container_cpu_usage_seconds_total{job="cadvisor", image!=""}[5m])) * on (cluster, namespace, pod) group_left(node) topk by (cluster, namespace, pod) (  1, max by(cluster, namespace, pod, node) (kube_pod_info{node!=""}))'
      }
      {
        record: 'node_namespace_pod_container:container_memory_working_set_bytes'
        expression: 'container_memory_working_set_bytes{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))'
      }
      {
        record: 'node_namespace_pod_container:container_memory_rss'
        expression: 'container_memory_rss{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))'
      }
      {
        record: 'node_namespace_pod_container:container_memory_cache'
        expression: 'container_memory_cache{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))'
      }
      {
        record: 'node_namespace_pod_container:container_memory_swap'
        expression: 'container_memory_swap{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))'
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_requests'
        expression: 'kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))'
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_requests:sum'
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))'
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_requests'
        expression: 'kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))'
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_requests:sum'
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))'
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_limits'
        expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))'
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_limits:sum'
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))'
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_limits'
        expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) ( (kube_pod_status_phase{phase=~"Pending|Running"} == 1) )'
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_limits:sum'
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))'
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    label_replace(      kube_pod_owner{job="kube-state-metrics", owner_kind="ReplicaSet"},      "replicaset", "$1", "owner_name", "(.*)"    ) * on(replicaset, namespace) group_left(owner_name) topk by(replicaset, namespace) (      1, max by (replicaset, namespace, owner_name) (        kube_replicaset_owner{job="kube-state-metrics"}      )    ),    "workload", "$1", "owner_name", "(.*)"  ))'
        labels: {
          workload_type: 'deployment'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="DaemonSet"},    "workload", "$1", "owner_name", "(.*)"  ))'
        labels: {
          workload_type: 'daemonset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="StatefulSet"},    "workload", "$1", "owner_name", "(.*)"  ))'
        labels: {
          workload_type: 'statefulset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="Job"},    "workload", "$1", "owner_name", "(.*)"  ))'
        labels: {
          workload_type: 'job'
        }
      }
      {
        record: ':node_memory_MemAvailable_bytes:sum'
        expression: 'sum(  node_memory_MemAvailable_bytes{job="node"} or  (    node_memory_Buffers_bytes{job="node"} +    node_memory_Cached_bytes{job="node"} +    node_memory_MemFree_bytes{job="node"} +    node_memory_Slab_bytes{job="node"}  )) by (cluster)'
      }
      {
        record: 'cluster:node_cpu:ratio_rate5m'
        expression: 'sum(rate(node_cpu_seconds_total{job="node",mode!="idle",mode!="iowait",mode!="steal"}[5m])) by (cluster) /count(sum(node_cpu_seconds_total{job="node"}) by (cluster, instance, cpu)) by (cluster)'
      }
    ]
  }
}

resource grafana_resource 'Microsoft.Dashboard/grafana@2023-09-01' = {
  name: grafana_name
  location: location
  sku: {
    name: 'Standard'
  }
  identity: {
       type: 'SystemAssigned'
  }
  properties: {
    grafanaIntegrations: {
      azureMonitorWorkspaceIntegrations: [
        {
          azureMonitorWorkspaceResourceId: monitor_workspace_resource.id
        }
      ]
    }
  }
}

var MonitoringDataReaderRoleId = 'b0d8363b-8ddd-447d-831f-62ca05bff136'
resource monitoring_data_reader_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(monitor_workspace_resource.id, 'Monitoring Data Reader', grafana_resource.id)
  scope: monitor_workspace_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', MonitoringDataReaderRoleId)
    principalId: grafana_resource.identity.principalId
  }
}

var MonitoringReaderRoleId = '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
resource monitoring_reader_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, 'Monitoring Reader', grafana_resource.id)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', MonitoringReaderRoleId)
    principalId: grafana_resource.identity.principalId
  }
}

var myId = '620d2777-78d3-4f42-8cbd-cc834bbcf9cb'
var GrafanaAdminRoleId = '22926164-76b3-42b3-bc55-97df8dab3e41'
resource grafana_admin_role_assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(grafana_resource.id, 'Grafana Admin', myId)
  scope: grafana_resource
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', GrafanaAdminRoleId)
    principalId: myId
  }
}
