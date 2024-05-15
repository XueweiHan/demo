package main

import (
	"fmt"
	"math/rand"
	"time"
)





func main() {
	fmt.Println("Hello, world!")



	for {
		n := 0
		fmt.Scanf("%d\n", &n)
		if n == 0 {
			break
		}

		start := time.Now()

		resultCh := make(chan int)
		timeout := false
		go func() {
			r := 0
			for !timeout {
				r += rand.Intn(100)
			}
			resultCh <- r
		}()
		time.Sleep(time.Duration(n) * time.Millisecond)
		timeout = true
		result := <-resultCh

		duration := time.Since(start)

		fmt.Println("result:   ", result)
		fmt.Println("duration: ", duration)
	}
}
