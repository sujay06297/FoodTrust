# 文件索引

這份是 `docs/` 的入口，說明每份文件的用途與更新方式，避免要找資訊時不知道該看哪一份。專案總覽與快速開始見根目錄 [README.md](../README.md)。

| 想找什麼 | 看哪份文件 | 更新方式 |
| --- | --- | --- |
| 產品規格本體（功能需求、評分模型、資料表設計、API 範例） | [../FoodTrust_project_spec.md](../FoodTrust_project_spec.md) | 規格本體異動才修改 |
| 現在的專案狀態（架構、已完成/未完成功能、目前部署阻塞、下一步待辦） | [handoff.md](handoff.md) | 覆寫式，只反映當下狀態，不疊加日期段落 |
| 歷史演進紀錄（每次功能 slice、部署里程碑的時間軸） | [devlog.md](devlog.md) | 只增不改，依日期往下加 |
| 重要架構/技術決策的完整脈絡（為什麼選這個方案、考慮過什麼替代方案） | [adr/](adr/) | 一個決策一個檔案，寫完不回頭改；devlog 只留摘要並連過來 |
| 部署與維運操作清單（例如 AWS 資源退場） | [ops/](ops/) | 操作導向，隨每次檢查更新 |
| 舊版/已淘汰文件的封存 | [archive/](archive/) | 只放可用 git diff 檢視的文字型資料；不進 repo 的原則見 [archive/README.md](archive/README.md) |

## 什麼不該放進這個 repo

- Word/PDF 等 binary 文件備份 —— 沒辦法 diff，只會讓 repo 累積不透明的歷史。若真的需要留存，放外部備份位置，並在 [archive/README.md](archive/README.md) 記錄檔名、日期與用途。
- 手動抄寫的 git commit 清單 —— 近期 commit 請直接查 `git log` / `git status`，寫進文件裡只會過時。
- 一次性 debug 過程的完整貼文 —— 只留結論與修法，過程留在 commit message 或 PR 討論。

## 新開一份文件之前先想
ppp
- 這是「現在的狀態」還是「發生過的事件」？前者進 [handoff.md](handoff.md)，後者進 [devlog.md](devlog.md)。
- 這是需要說明「為什麼選這個方案」的決策嗎？是的話拆一份 ADR，不要只塞在 devlog 裡。
- 這是操作型清單（步驟、指令、檢查表）嗎？放 [ops/](ops/)。
