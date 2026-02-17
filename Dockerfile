# ใช้ Image ของ .NET 9 SDK เพื่อ Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ก๊อปปี้ไฟล์โปรเจกต์ทั้งหมดเข้า Docker
COPY . .

# สั่ง Restore และ Publish ออกมาเป็นไฟล์ .dll
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ใช้ Image ตัวเล็ก (Runtime) เพื่อรันจริง
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# ตั้งค่า Port ให้ตรงกับ Render
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# ⚠️ บรรทัดนี้สำคัญที่สุด! ⚠️ 
# Render จะสั่งรันไฟล์ .dll ตัวแรกที่มันเจอในโฟลเดอร์ (จะได้ไม่ต้องมาแก้ชื่อเอง)
ENTRYPOINT ["sh", "-c", "dotnet *.dll"]