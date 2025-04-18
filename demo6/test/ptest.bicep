targetScope = 'managementGroup'
param clusterResourceId string
param clusterLocation string

@description('Resource Id of the existing Azure Monitor Workspace')
@metadata({ displayName: 'Resource Id of the existing Azure Monitor Workspace' })
param azureMonitorWorkspaceResourceId string

@description('Location of the existing Azure Monitor Workspace')
@metadata({ displayName: 'Location of the existing Azure Monitor Workspace' })
param azureMonitorWorkspaceLocation string

@description('Comma-separated list of additional Kubernetes label keys that will be used in the resource\' labels metric. By default the metric contains only name and namespace labels. To include additional labels provide a list of resource names in their plural form and Kubernetes label keys you would like to allow for them (Example: \'namespaces=[k8s-label-1,k8s-label-n,...],pods=[app],...)\'. A single \'*\' can be provided per resource instead to allow any labels, but that has severe performance implications (Example: \'pods=[*]\'). Additionally, an asterisk (*) can be provided as a key, which will resolve to all resources, i.e., assuming \'--resources=deployments,pods\', \'*=[*]\' will resolve to \'deployments=[*],pods=[*]\'')
@metadata({ displayName: 'Comma-separated list of additional Kubernetes label keys' })
param metricLabelsAllowlist string = ''

@description('Comma-separated list of Kubernetes annotations keys that will be used in the resource\' annotations metric. By default no annotations are collected. To include additional annotations provide a list of resource names in their plural form and Kubernetes annotation keys you would like to allow for them (Example: \'namespaces=[kubernetes.io/team,...],pods=[kubernetes.io/team],...)\'. A single \'*\' can be provided per resource instead to allow any annotations, but that has severe performance implications (Example: \'pods=[*]\')')
@metadata({ displayName: 'Comma-separated list of Kubernetes annotations keys' })
param metricAnnotationsAllowList string = ''

@description('Enable recording rule group for windows metrics')
@metadata({ displayName: 'Enable recording rule group for windows metrics' })
param enableWindowsRecordingRules bool

var clusterName = 'test'
var nodeRecordingRuleGroup = 'test'
var kubernetesRecordingRuleGroup = 'test'
var nodeRecordingRuleGroupWin = 'test'
var nodeAndKubernetesRecordingRuleGroupWin = 'test'
var dceName = 'test'
var dcrName = 'test'
var dcraName = 'test'
var clusterSubscriptionId = 'test'
var clusterResourceGroup = 'test'
var nodeRecordingRuleGroupName = 'test'
var nodeRecordingRuleGroupDescription = 'test'
var version = '1.0.0.0'
var kubernetesRecordingRuleGroupName = 'test'
var kubernetesRecordingRuleGroupDescription = 'test'
var nodeRecordingRuleGroupNameWin = 'test'
var RecordingRuleGroupDescriptionWin = 'test'
var nodeAndKubernetesRecordingRuleGroupNameWin = 'test'
var policyName = 'audit-resource-tag-and-value-format-pd'
var policyDisplayName = 'Audit a tag and its value format on resources'
var policyDescription = 'Audits existence of a tag and its value format. Does not apply to resource groups.'




resource policy 'Microsoft.Authorization/policyDefinitions@2023-04-01' = {
  name: policyName
  properties: {
    displayName: policyDisplayName
    description: policyDescription
    policyType: 'Custom'
    mode: 'Indexed'
    metadata: {
      category: 'Tags'
    }
    parameters: {
      azureMonitorWorkspaceResourceId: {
        type: 'string'
        metadata: {
          displayName: 'Resource Id of the existing Azure Monitor Workspace'
          description: 'Resource Id of the existing Azure Monitor Workspace'
        }
      }
      azureMonitorWorkspaceLocation: {
        type: 'string'
        metadata: {
          displayName: 'Location of the existing Azure Monitor Workspace'
          description: 'Location of the existing Azure Monitor Workspace'
        }
      }
      metricLabelsAllowlist: {
        type: 'string'
        defaultValue: ''
        metadata: {
          displayName: 'Comma-separated list of additional Kubernetes label keys'
          description: 'Comma-separated list of additional Kubernetes label keys that will be used in the resource\' labels metric. By default the metric contains only name and namespace labels. To include additional labels provide a list of resource names in their plural form and Kubernetes label keys you would like to allow for them (Example: \'namespaces=[k8s-label-1,k8s-label-n,...],pods=[app],...)\'. A single \'*\' can be provided per resource instead to allow any labels, but that has severe performance implications (Example: \'pods=[*]\'). Additionally, an asterisk (*) can be provided as a key, which will resolve to all resources, i.e., assuming \'--resources=deployments,pods\', \'*=[*]\' will resolve to \'deployments=[*],pods=[*]\''
        }
      }
      metricAnnotationsAllowList: {
        type: 'string'
        defaultValue: ''
        metadata: {
          displayName: 'Comma-separated list of Kubernetes annotations keys'
          description: 'Comma-separated list of Kubernetes annotations keys that will be used in the resource\' annotations metric. By default no annotations are collected. To include additional annotations provide a list of resource names in their plural form and Kubernetes annotation keys you would like to allow for them (Example: \'namespaces=[kubernetes.io/team,...],pods=[kubernetes.io/team],...)\'. A single \'*\' can be provided per resource instead to allow any annotations, but that has severe performance implications (Example: \'pods=[*]\')'
        }
      }
      enableWindowsRecordingRules: {
        type: 'boolean'
        metadata: {
          displayName: 'Enable recording rule group for windows metrics'
          description: 'Enable recording rule group for windows metrics'
        }
      }
    }
    policyRule: {
      if: {
        field: 'type'
        equals: 'Microsoft.ContainerService/managedClusters'
      }
      then: {
        effect: 'deployIfNotExists'
        details: {
          type: 'Microsoft.ContainerService/managedClusters'
          name: '[field(\'name\')]'
          roleDefinitionIds: [
            '/providers/Microsoft.Authorization/roleDefinitions/b24988ac-6180-42a0-ab88-20f7382dd24c'
          ]
          existenceCondition: {
            field: 'Microsoft.ContainerService/managedClusters/azureMonitorProfile.metrics.enabled'
            equals: 'true'
          }
          deployment: {
            properties: {
              mode: 'incremental'
              template: {
                '$schema': 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#'
                contentVersion: '1.0.0.0'
                parameters: {
                  azureMonitorWorkspaceResourceId: {
                    type: 'string'
                  }
                  azureMonitorWorkspaceLocation: {
                    type: 'string'
                    defaultValue: ''
                  }
                  metricLabelsAllowlist: {
                    type: 'string'
                    defaultValue: ''
                  }
                  metricAnnotationsAllowList: {
                    type: 'string'
                    defaultValue: ''
                  }
                  enableWindowsRecordingRules: {
                    type: 'bool'
                  }
                  clusterResourceId: {
                    type: 'string'
                  }
                  clusterLocation: {
                    type: 'string'
                  }
                }
                variables: {
                  clusterSubscriptionId: split(clusterResourceId, '/')[2]
                  clusterResourceGroup: split(clusterResourceId, '/')[4]
                  clusterName: split(clusterResourceId, '/')[8]
                  dceName: 'MSProm-${azureMonitorWorkspaceLocation}-${clusterName}'
                  dcrName: 'MSProm-${azureMonitorWorkspaceLocation}-${clusterName}'
                  dcraName: 'MSProm-${clusterLocation}-${clusterName}'
                  nodeRecordingRuleGroup: 'NodeRecordingRulesRuleGroup-'
                  nodeRecordingRuleGroupName: concat(nodeRecordingRuleGroup, clusterName)
                  nodeRecordingRuleGroupDescription: 'Node Recording Rules RuleGroup'
                  kubernetesRecordingRuleGroup: 'KubernetesRecordingRulesRuleGroup-'
                  kubernetesRecordingRuleGroupName: concat(kubernetesRecordingRuleGroup, clusterName)
                  kubernetesRecordingRuleGroupDescription: 'Kubernetes Recording Rules RuleGroup'
                  nodeRecordingRuleGroupWin: 'NodeRecordingRulesRuleGroup-Win-'
                  nodeAndKubernetesRecordingRuleGroupWin: 'NodeAndKubernetesRecordingRulesRuleGroup-Win-'
                  nodeRecordingRuleGroupNameWin: concat(nodeRecordingRuleGroupWin, clusterName)
                  nodeAndKubernetesRecordingRuleGroupNameWin: concat(
                    nodeAndKubernetesRecordingRuleGroupWin,
                    clusterName
                  )
                  RecordingRuleGroupDescriptionWin: 'Kubernetes Recording Rules RuleGroup for Win'
                  version: ' - 0.1'
                }
                resources: [
                  {
                    type: 'Microsoft.Insights/dataCollectionEndpoints'
                    apiVersion: '2022-06-01'
                    name: dceName
                    location: azureMonitorWorkspaceLocation
                    kind: 'Linux'
                    properties: {}
                  }
                  {
                    type: 'Microsoft.Insights/dataCollectionRules'
                    apiVersion: '2022-06-01'
                    name: dcrName
                    location: azureMonitorWorkspaceLocation
                    kind: 'Linux'
                    properties: {
                      dataCollectionEndpointId: resourceId('Microsoft.Insights/dataCollectionEndpoints/', dceName)
                      dataFlows: [
                        {
                          destinations: [
                            'MonitoringAccount1'
                          ]
                          streams: [
                            'Microsoft-PrometheusMetrics'
                          ]
                        }
                      ]
                      dataSources: {
                        prometheusForwarder: [
                          {
                            name: 'PrometheusDataSource'
                            streams: [
                              'Microsoft-PrometheusMetrics'
                            ]
                            labelIncludeFilter: {}
                          }
                        ]
                      }
                      description: 'DCR for Azure Monitor Metrics Profile (Managed Prometheus)'
                      destinations: {
                        monitoringAccounts: [
                          {
                            accountResourceId: azureMonitorWorkspaceResourceId
                            name: 'MonitoringAccount1'
                          }
                        ]
                      }
                    }
                    dependsOn: [
                      resourceId('Microsoft.Insights/dataCollectionEndpoints/', dceName)
                    ]
                  }
                  {
                    type: 'Microsoft.Resources/deployments'
                    name: 'azuremonitormetrics-dcra-${uniqueString(clusterResourceId)}'
                    apiVersion: '2017-05-10'
                    subscriptionId: clusterSubscriptionId
                    resourceGroup: clusterResourceGroup
                    dependsOn: [
                      resourceId('Microsoft.Insights/dataCollectionEndpoints/', dceName)
                      resourceId('Microsoft.Insights/dataCollectionRules', dcrName)
                    ]
                    properties: {
                      mode: 'Incremental'
                      template: {
                        '$schema': 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#'
                        contentVersion: '1.0.0.0'
                        parameters: {}
                        variables: {}
                        resources: [
                          {
                            type: 'Microsoft.ContainerService/managedClusters/providers/dataCollectionRuleAssociations'
                            name: '${clusterName}/microsoft.insights/${dcraName}'
                            apiVersion: '2022-06-01'
                            location: clusterLocation
                            properties: {
                              description: 'Association of data collection rule. Deleting this association will break the data collection for this AKS Cluster.'
                              dataCollectionRuleId: resourceId('Microsoft.Insights/dataCollectionRules', dcrName)
                            }
                          }
                        ]
                      }
                      parameters: {}
                    }
                  }
                  {
                    type: 'Microsoft.Resources/deployments'
                    name: 'azuremonitormetrics-profile--${uniqueString(clusterResourceId)}'
                    apiVersion: '2017-05-10'
                    subscriptionId: clusterSubscriptionId
                    resourceGroup: clusterResourceGroup
                    dependsOn: [
                      'azuremonitormetrics-dcra-${uniqueString(clusterResourceId)}'
                    ]
                    properties: {
                      mode: 'Incremental'
                      template: {
                        '$schema': 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#'
                        contentVersion: '1.0.0.0'
                        parameters: {}
                        variables: {}
                        resources: [
                          {
                            name: clusterName
                            type: 'Microsoft.ContainerService/managedClusters'
                            location: clusterLocation
                            apiVersion: '2023-01-01'
                            properties: {
                              mode: 'Incremental'
                              id: clusterResourceId
                              azureMonitorProfile: {
                                metrics: {
                                  enabled: true
                                  kubeStateMetrics: {
                                    metricLabelsAllowlist: metricLabelsAllowlist
                                    metricAnnotationsAllowList: metricAnnotationsAllowList
                                  }
                                }
                              }
                            }
                          }
                        ]
                      }
                      parameters: {}
                    }
                  }
                  {
                    name: nodeRecordingRuleGroupName
                    type: 'Microsoft.AlertsManagement/prometheusRuleGroups'
                    apiVersion: '2023-03-01'
                    location: azureMonitorWorkspaceLocation
                    properties: {
                      description: concat(nodeRecordingRuleGroupDescription, version)
                      scopes: [
                        azureMonitorWorkspaceResourceId
                      ]
                      clusterName: clusterName
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
                  {
                    name: kubernetesRecordingRuleGroupName
                    type: 'Microsoft.AlertsManagement/prometheusRuleGroups'
                    apiVersion: '2023-03-01'
                    location: azureMonitorWorkspaceLocation
                    properties: {
                      description: concat(kubernetesRecordingRuleGroupDescription, version)
                      scopes: [
                        azureMonitorWorkspaceResourceId
                      ]
                      clusterName: clusterName
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
                  {
                    name: nodeRecordingRuleGroupNameWin
                    type: 'Microsoft.AlertsManagement/prometheusRuleGroups'
                    apiVersion: '2023-03-01'
                    location: azureMonitorWorkspaceLocation
                    properties: {
                      description: concat(RecordingRuleGroupDescriptionWin, version)
                      scopes: [
                        azureMonitorWorkspaceResourceId
                      ]
                      enabled: enableWindowsRecordingRules
                      clusterName: clusterName
                      interval: 'PT1M'
                      rules: [
                        {
                          record: 'node:windows_node:sum'
                          expression: 'count (windows_system_system_up_time{job="windows-exporter"})'
                        }
                        {
                          record: 'node:windows_node_num_cpu:sum'
                          expression: 'count by (instance) (sum by (instance, core) (windows_cpu_time_total{job="windows-exporter"}))'
                        }
                        {
                          record: ':windows_node_cpu_utilisation:avg5m'
                          expression: '1 - avg(rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))'
                        }
                        {
                          record: 'node:windows_node_cpu_utilisation:avg5m'
                          expression: '1 - avg by (instance) (rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))'
                        }
                        {
                          record: ':windows_node_memory_utilisation:'
                          expression: '1 -sum(windows_memory_available_bytes{job="windows-exporter"})/sum(windows_os_visible_memory_bytes{job="windows-exporter"})'
                        }
                        {
                          record: ':windows_node_memory_MemFreeCached_bytes:sum'
                          expression: 'sum(windows_memory_available_bytes{job="windows-exporter"} + windows_memory_cache_bytes{job="windows-exporter"})'
                        }
                        {
                          record: 'node:windows_node_memory_totalCached_bytes:sum'
                          expression: '(windows_memory_cache_bytes{job="windows-exporter"} + windows_memory_modified_page_list_bytes{job="windows-exporter"} + windows_memory_standby_cache_core_bytes{job="windows-exporter"} + windows_memory_standby_cache_normal_priority_bytes{job="windows-exporter"} + windows_memory_standby_cache_reserve_bytes{job="windows-exporter"})'
                        }
                        {
                          record: ':windows_node_memory_MemTotal_bytes:sum'
                          expression: 'sum(windows_os_visible_memory_bytes{job="windows-exporter"})'
                        }
                        {
                          record: 'node:windows_node_memory_bytes_available:sum'
                          expression: 'sum by (instance) ((windows_memory_available_bytes{job="windows-exporter"}))'
                        }
                        {
                          record: 'node:windows_node_memory_bytes_total:sum'
                          expression: 'sum by (instance) (windows_os_visible_memory_bytes{job="windows-exporter"})'
                        }
                        {
                          record: 'node:windows_node_memory_utilisation:ratio'
                          expression: '(node:windows_node_memory_bytes_total:sum - node:windows_node_memory_bytes_available:sum) / scalar(sum(node:windows_node_memory_bytes_total:sum))'
                        }
                        {
                          record: 'node:windows_node_memory_utilisation:'
                          expression: '1 - (node:windows_node_memory_bytes_available:sum / node:windows_node_memory_bytes_total:sum)'
                        }
                        {
                          record: 'node:windows_node_memory_swap_io_pages:irate'
                          expression: 'irate(windows_memory_swap_page_operations_total{job="windows-exporter"}[5m])'
                        }
                        {
                          record: ':windows_node_disk_utilisation:avg_irate'
                          expression: 'avg(irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m]))'
                        }
                        {
                          record: 'node:windows_node_disk_utilisation:avg_irate'
                          expression: 'avg by (instance) ((irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m])))'
                        }
                      ]
                    }
                  }
                  {
                    name: nodeAndKubernetesRecordingRuleGroupNameWin
                    type: 'Microsoft.AlertsManagement/prometheusRuleGroups'
                    apiVersion: '2023-03-01'
                    location: azureMonitorWorkspaceLocation
                    properties: {
                      description: concat(RecordingRuleGroupDescriptionWin, version)
                      scopes: [
                        azureMonitorWorkspaceResourceId
                      ]
                      enabled: enableWindowsRecordingRules
                      clusterName: clusterName
                      interval: 'PT1M'
                      rules: [
                        {
                          record: 'node:windows_node_filesystem_usage:'
                          expression: 'max by (instance,volume)((windows_logical_disk_size_bytes{job="windows-exporter"} - windows_logical_disk_free_bytes{job="windows-exporter"}) / windows_logical_disk_size_bytes{job="windows-exporter"})'
                        }
                        {
                          record: 'node:windows_node_filesystem_avail:'
                          expression: 'max by (instance, volume) (windows_logical_disk_free_bytes{job="windows-exporter"} / windows_logical_disk_size_bytes{job="windows-exporter"})'
                        }
                        {
                          record: ':windows_node_net_utilisation:sum_irate'
                          expression: 'sum(irate(windows_net_bytes_total{job="windows-exporter"}[5m]))'
                        }
                        {
                          record: 'node:windows_node_net_utilisation:sum_irate'
                          expression: 'sum by (instance) ((irate(windows_net_bytes_total{job="windows-exporter"}[5m])))'
                        }
                        {
                          record: ':windows_node_net_saturation:sum_irate'
                          expression: 'sum(irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m])) + sum(irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m]))'
                        }
                        {
                          record: 'node:windows_node_net_saturation:sum_irate'
                          expression: 'sum by (instance) ((irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m]) + irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m])))'
                        }
                        {
                          record: 'windows_pod_container_available'
                          expression: 'windows_container_available{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'windows_container_total_runtime'
                          expression: 'windows_container_cpu_usage_seconds_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'windows_container_memory_usage'
                          expression: 'windows_container_memory_usage_commit_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'windows_container_private_working_set_usage'
                          expression: 'windows_container_memory_usage_private_working_set_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'windows_container_network_received_bytes_total'
                          expression: 'windows_container_network_receive_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'windows_container_network_transmitted_bytes_total'
                          expression: 'windows_container_network_transmit_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)'
                        }
                        {
                          record: 'kube_pod_windows_container_resource_memory_request'
                          expression: 'max by (namespace, pod, container) (kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)'
                        }
                        {
                          record: 'kube_pod_windows_container_resource_memory_limit'
                          expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)'
                        }
                        {
                          record: 'kube_pod_windows_container_resource_cpu_cores_request'
                          expression: 'max by (namespace, pod, container) ( kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)'
                        }
                        {
                          record: 'kube_pod_windows_container_resource_cpu_cores_limit'
                          expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)'
                        }
                        {
                          record: 'namespace_pod_container:windows_container_cpu_usage_seconds_total:sum_rate'
                          expression: 'sum by (namespace, pod, container) (rate(windows_container_total_runtime{}[5m]))'
                        }
                      ]
                    }
                  }
                ]
              }
              parameters: {
                azureMonitorWorkspaceResourceId: {
                  value: azureMonitorWorkspaceResourceId
                }
                azureMonitorWorkspaceLocation: {
                  value: azureMonitorWorkspaceLocation
                }
                metricLabelsAllowlist: {
                  value: metricLabelsAllowlist
                }
                metricAnnotationsAllowList: {
                  value: metricAnnotationsAllowList
                }
                enableWindowsRecordingRules: {
                  value: enableWindowsRecordingRules
                }
                clusterResourceId: {
                  value: {
                    field: 'id'
                  }
                }
                clusterLocation: {
                  value: {
                    field: 'location'
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
