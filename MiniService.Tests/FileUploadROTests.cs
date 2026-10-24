// #49 FileController/UploadFileRO — KHÔNG port (NO_BACKEND).
// Toàn bộ phần lưu file nằm ở dịch vụ ngoài `_strConfig_FileLocal_Url` (Web.config:151 = http://localhost:19668/)
// qua FileService.WA_File_SaveUploadFileRO (BizService\Services\FileService.cs:34 PostDataCG "File/SaveUploadFileRO").
// Không có backend nội bộ trong 11.BackEnd\V10 để port. Xem NO_BACKEND.flag + conflicts/49_conflict.md.
// File test cũ đã được gỡ (một lượt trước đã port sai dựa trên thông tin không tồn tại).
