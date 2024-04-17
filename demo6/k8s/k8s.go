package k8s

import (
	"context"
	"fmt"
	"os"
	"path/filepath"

	v1 "k8s.io/apimachinery/pkg/apis/meta/v1"
	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
	"k8s.io/client-go/tools/clientcmd"
)

func GetK8sPods() string {
	output := "Get Kubernetes pods\n"

	kubeConfig, err := rest.InClusterConfig()
	if err != nil {
		var userHomeDir string
		userHomeDir, err = os.UserHomeDir()
		if err != nil {
			output += fmt.Sprintf("error getting user home dir: %v\n", err)
			return output
		}
		kubeConfigPath := filepath.Join(userHomeDir, ".kube", "config")
		output += fmt.Sprintf("Using kubeconfig: %s\n", kubeConfigPath)

		kubeConfig, err = clientcmd.BuildConfigFromFlags("", kubeConfigPath)
	}
	if err != nil {
		output += fmt.Sprintf("error getting Kubernetes config: %v\n", err)
		return output
	}

	clientset, err := kubernetes.NewForConfig(kubeConfig)
	if err != nil {
		output += fmt.Sprintf("error getting Kubernetes clientset: %v\n", err)
		return output
	}

	// pods, err := clientset.CoreV1().Pods("kube-system").List(context.Background(), v1.ListOptions{})
	pods, err := clientset.CoreV1().Pods("default").List(context.Background(), v1.ListOptions{})
	if err != nil {
		output += fmt.Sprintf("error getting pods: %v\n", err)
		return output
	}

	for _, pod := range pods.Items {
		output += fmt.Sprintf("Pod name: %s  Namespace: %s\n", pod.Name, pod.Namespace)
		for _, image := range pod.Spec.Containers {
			output += fmt.Sprintf("  Image: %s\n", image.Image)
		}
	}

	return output
}
