package main

import (
	"embed"
	"encoding/json"
	"fmt"
	"io/fs"
	"log"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"time"
)

//go:embed web/*
var webFiles embed.FS

const (
	serverAddr   = ":8080"
	appURL       = "http://localhost:8080"
	mockPhone    = "7845550512"
	mockPassword = "7845550512"
)

type loginRequest struct {
	Phone    string `json:"phone"`
	Password string `json:"password"`
}

type apiResponse struct {
	Success bool   `json:"success"`
	Message string `json:"message"`
}

func main() {
	webRoot, err := fs.Sub(webFiles, "web")
	if err != nil {
		log.Fatal(err)
	}

	mux := http.NewServeMux()
	mux.Handle("/", http.FileServer(http.FS(webRoot)))
	mux.HandleFunc("POST /api/login", handleLogin)
	mux.HandleFunc("POST /api/print", handlePrint)

	if os.Getenv("PAWN_NO_BROWSER") != "1" {
		go func() {
			time.Sleep(350 * time.Millisecond)
			if err := openBrowser(appURL); err != nil {
				log.Printf("Open %s manually. Browser launch failed: %v", appURL, err)
			}
		}()
	}

	log.Printf("Pawn Go PoC running at %s", appURL)
	log.Fatal(http.ListenAndServe(serverAddr, mux))
}

func handleLogin(w http.ResponseWriter, r *http.Request) {
	var req loginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		writeJSON(w, http.StatusBadRequest, apiResponse{Success: false, Message: "Invalid login payload."})
		return
	}

	req.Phone = strings.TrimSpace(req.Phone)
	if req.Phone == mockPhone && req.Password == mockPassword {
		writeJSON(w, http.StatusOK, apiResponse{Success: true, Message: "Login successful."})
		return
	}

	writeJSON(w, http.StatusUnauthorized, apiResponse{Success: false, Message: "Phone or password is incorrect."})
}

func handlePrint(w http.ResponseWriter, r *http.Request) {
	receipt := buildReceipt()

	fmt.Println(receipt)
	path := receiptPath()
	if err := os.WriteFile(path, []byte(receipt), 0644); err != nil {
		log.Printf("Receipt write failed: %v", err)
		writeJSON(w, http.StatusInternalServerError, apiResponse{Success: false, Message: "Could not write receipt file."})
		return
	}

	writeJSON(w, http.StatusOK, apiResponse{Success: true, Message: "Dummy receipt printed and saved."})
}

func buildReceipt() string {
	now := time.Now().Format("02 Jan 2006 03:04 PM")
	return fmt.Sprintf(`================================
        PAWN BROKER ERP
        DUMMY PAWN TICKET
================================
Ticket No : POC-0001
Date      : %s
Customer  : Ramesh Kumar
Phone     : 7845550512

Item      : Gold Chain
Weight    : 12.50 g
Amount    : Rs. 45,000.00
Interest  : 2.00%% monthly

Operator  : Demo User
Status    : Printed from Go backend
================================
`, now)
}

func writeJSON(w http.ResponseWriter, status int, payload apiResponse) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	if err := json.NewEncoder(w).Encode(payload); err != nil {
		log.Printf("JSON response failed: %v", err)
	}
}

func openBrowser(url string) error {
	switch runtime.GOOS {
	case "darwin":
		if _, err := os.Stat("/Applications/Google Chrome.app"); err == nil {
			return exec.Command("open", "-na", "Google Chrome", "--args", "--app="+url).Start()
		}
		return exec.Command("open", url).Start()
	case "windows":
		if err := exec.Command("cmd", "/c", "start", "", "msedge", "--app="+url).Start(); err == nil {
			return nil
		}
		if err := exec.Command("cmd", "/c", "start", "", "chrome", "--app="+url).Start(); err == nil {
			return nil
		}
		return exec.Command("rundll32", "url.dll,FileProtocolHandler", url).Start()
	default:
		return exec.Command("xdg-open", url).Start()
	}
}

func receiptPath() string {
	executable, err := os.Executable()
	if err != nil {
		return "dummy_receipt.txt"
	}

	dir := filepath.Dir(executable)
	if strings.HasSuffix(filepath.ToSlash(dir), ".app/Contents/MacOS") {
		bundleDir := filepath.Clean(filepath.Join(dir, "..", ".."))
		return filepath.Join(filepath.Dir(bundleDir), "dummy_receipt.txt")
	}

	return filepath.Join(dir, "dummy_receipt.txt")
}
