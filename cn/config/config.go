package config

import "os"

var (
	Tag string
)

func init() {
	// get the build tag
	if t, ok := os.LookupEnv("TAG"); ok {
		Tag = t
	}
}
