package model

import (
	"encoding/json"
	"fmt"
	"reflect"
	"strings"
)

// shared function to marshall json object in string field
func MarshalJSONField(o any) ([]byte, error) {
	var fieldStrs []string

	v := reflect.ValueOf(o)
	t := v.Type()
	for i := 0; i < t.NumField(); i++ {
		fieldName := t.Field(i).Name

		obj := v.Field(i).Interface()
		tag := t.Field(i).Tag.Get("marshal")
		if tag == "json" {
			var jsonObj map[string]any
			data := []byte(obj.(string))
			if len(data) > 0 {
				err := json.Unmarshal(data, &jsonObj)
				if err != nil {
					return nil, err
				}
				obj = jsonObj
			}
		}

		b, err := json.Marshal(obj)
		if err != nil {
			return b, err
		}

		fieldStrs = append(fieldStrs, fmt.Sprintf(`"%s": %s`, fieldName, string(b)))
	}

	s := fmt.Sprintf("{%s}", strings.Join(fieldStrs, ","))

	return []byte(s), nil
}
