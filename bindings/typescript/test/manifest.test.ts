require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { InternalManifest, Manifest } from "../src/manifest";

const manifest = (test_data as unknown as any).election.manifest;

const internalManifest = (test_data as unknown as any).election
  .internal_manifest;

describe("Manifest Tests", () => {
  it("should convert from json", async () => {
    const expected = JSON.stringify(manifest);
    const result = await Manifest.fromJson(expected);
    assert.isTrue(result.toJson().includes(manifest.election_scope_id));
  });
});

describe("InternalManifest Tests", () => {
  it("should convert from json", async () => {
    const expected = JSON.stringify(internalManifest);
    const result = await InternalManifest.fromJson(expected);
    assert.isTrue(result.toJson().includes(internalManifest.manifest_hash));
  });
});
