spacetime generate -l csharp -o ../client/Scripts/SpacetimeDb -d -n StdbCraft.Scripts.SpacetimeDb

# shellcheck disable=SC2044
for f in $(find ../client/Scripts/SpacetimeDb -name '*.cs'); do
  sed -e s/\(SpacetimeDBClient\.TableOp\ op\,\ /\(/g -i "$f"
  sed -e s/\?\.Invoke\(op\,\ /\?\.Invoke\(/g -i "$f"
done
