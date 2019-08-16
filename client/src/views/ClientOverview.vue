<template>
  <div class="animated component-wrapper">
    <div class="toolbar">
      <label>Client</label>
      <b-form-select
        id="client1Select"
        value="Please select"
        :options="availableClients"
        v-model="selectedClient"
        @change="loadSchema(selectedClient)"
      ></b-form-select>
    </div>
    <b-alert :show="selectedClient.client">
      <div>Version: {{selectedClient.version}}</div>
      <div>Entitled: {{selectedClient.entitled ? 'True' : 'False'}}</div>
    </b-alert>

    <b-card title="Stored Procedures" v-if="modifiedStoredProcs.length > 0">
      <div v-for="(item, idx) in modifiedStoredProcs" :key="idx">
        <router-link
          :to="{ path : 'diff', query: { client: selectedClient.client, object: item.dbObject, version: selectedClient.version }}"
        >{{ item.message }}</router-link>
      </div>
    </b-card>
    <b-card title="Triggers" v-if="modifiedTriggers.length > 0">
      <div v-for="(item, idx) in modifiedTriggers" :key="idx">
        <router-link
          :to="{ path : 'diff', query: { client: selectedClient.client, object: item.dbObject, version: selectedClient.version }}"
        >{{ item.message }}</router-link>
      </div>
    </b-card>
    <b-card title="Tables" v-if="modifiedTables.length > 0">
      <div v-for="(item, idx) in modifiedTables" :key="idx">
        <router-link
          :to="{ path : 'diff', query: { client: selectedClient.client, object: item.dbObject, version: selectedClient.version }}"
        >{{ item.message }}</router-link>
      </div>
    </b-card>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "client-overview",
  components: {},
  data() {
    return {
      availableClients: [],
      overview: [],
      selectedClient: ""
    };
  },
  computed: {
    modifiedStoredProcs() {
      console.log(this.overview);
      return this.overview.filter(o => o.dbType === "P");
    },
    modifiedTriggers() {
      return this.overview.filter(o => o.dbType === "TR");
    },
    modifiedTables() {
      return this.overview.filter(o => o.dbType === "U");
    }
  },
  mounted() {
    axios.get("http://localhost:5000/api/clients").then(response => {
      this.availableClients = response.data.map(o => {
        return { text: o.client, value: o };
      });

      if (this.$route.query.client) {
        this.selectedClient = response.data.find(
          o => o.client === this.$route.query.client
        );
      }
    });
  },
  methods: {
    loadSchema(selectedClient) {
      axios
        .get(
          `http://localhost:5000/api/schema/${selectedClient.client}/overview`
        )
        .then(response => {
          this.overview = response.data;
        });
    }
  }
};
</script>

<style scoped lang="scss" >
.toolbar {
  padding: 6px 0 10px 0;
  display: flex;
  flex-direction: row;
  label {
    margin: 8px;
  }
}
</style>
