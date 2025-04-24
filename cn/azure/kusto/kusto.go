package kusto

import (
	"context"
	"log"

	"certnoti/azure/credential"
	"certnoti/model"

	"github.com/Azure/azure-kusto-go/kusto"
	"github.com/Azure/azure-kusto-go/kusto/data/errors"
	"github.com/Azure/azure-kusto-go/kusto/data/table"
	"github.com/Azure/azure-kusto-go/kusto/kql"
)

func QueryHelios() []model.InventoryCredSMARTCertInfo {

	// fmt.Fprint(w, "----Query Helios----\n")

	cerd := credential.GetAzureCredential()

	// kusto package is: github.com/Azure/azure-kusto-go/kusto
	kustoConnectionStringBuilder := kusto.NewConnectionStringBuilder("https://o365security.kusto.windows.net/")
	// Initialize a new kusto client using the default Azure credential
	kustoConnectionString := kustoConnectionStringBuilder.WithTokenCredential(cerd)

	client, err := kusto.New(kustoConnectionString)
	if err != nil {
		log.Println(err)
	}
	// Be sure to close the client when you're done. (Error handling omitted for brevity.)
	defer client.Close()

	ctx := context.Background()
	query := kql.New(model.QueryForInventoryCredSMARTCertInfo)
	database := "helios"

	params := kql.NewParameters()
	// You can see the generated parameters using the ToDeclarationString() method:

	iter, err := client.Query(ctx, database, query, kusto.QueryParameters(params))
	if err != nil {
		log.Println(err)
	}

	defer iter.Stop()

	// .Do() will call the function for every row in the table.
	// Replace flag indicates that the query result should be cleared and replaced with this row
	recs := []model.InventoryCredSMARTCertInfo{}

	//err = ReadKustoRow(iter, w)
	err = ReadKustoData(iter, &recs)
	if err != nil {
		log.Println(err)
	}

	return filterCertInfo(recs)
}

func ReadKustoData[T any](iter *kusto.RowIterator, recs *[]T) error {
	return iter.DoOnRowOrError(
		func(row *table.Row, e *errors.Error) error {
			if e != nil {
				return e
			}

			var rec T
			if err := row.ToStruct(&rec); err != nil {
				return err
			}

			if row.Replace {
				// Replace flag indicates that the query result should be cleared and replaced with this row
				*recs = (*recs)[:0]
			}

			*recs = append(*recs, rec)
			return nil
		},
	)
}

func filterCertInfo(certInfo []model.InventoryCredSMARTCertInfo) []model.InventoryCredSMARTCertInfo {
	var info []model.InventoryCredSMARTCertInfo
	for _, ci := range certInfo {
		if len(ci.ThumbprintSHA1) > 0 {
			info = append(info, ci)
		}
	}
	return info
}
